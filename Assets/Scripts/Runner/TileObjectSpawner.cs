


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectSpawner : MonoBehaviour
{
    public int laneCount = 3;
    public float laneDistance = 2.8f;

    [Header("Modules")]
    public CollectibleSpawner collectibleSpawner;
    public BoostSpawner boostSpawner;
    public DangerSpawner dangerSpawner;
    public PowerUpSpawner powerUpSpawner;
    public ProfessionSpawner professionSpawner;

    [Header("Safety")]
    public float professionSafeRadius = 3f;
    public float globalMinZDistance = 2f;

    [Header("Limits")]
    public int maxDangersPerTile = 1;
    public int maxCollectiblesPerTile = 6;
    public int maxBoostsPerTile = 1;
    public int maxPowerUpsPerTile = 1;

    private float lastDangerZ = -999f;
    public float dangerCooldown = 18f;

    private readonly List<float> professionZPositions = new();
    private float lastProfessionZ = -999f;
    private int tileCounter = 0;
    static int globalTileCounter = 0;
    private bool IsNearProfession(float z)
    {
        foreach (float pz in professionZPositions)
        {
            if (Mathf.Abs(pz - z) < professionSafeRadius)
                return true;
        }
        return false;
    }

    private void CleanupProfessionZ(float currentZ)
    {
        professionZPositions.RemoveAll(z => currentZ - z > 60f);
    }

    public void SpawnAll(Transform tile, float tileWorldZ)
    {
        globalTileCounter++;
        tileCounter++;

        CleanupProfessionZ(tileWorldZ);

        var comp = tile.GetComponent<TileLaneStateComponent>();
        if (comp == null)
        {
            comp = tile.gameObject.AddComponent<TileLaneStateComponent>();
            comp.Init(laneCount);
        }
        else
        {
            comp.Init(laneCount);
        }

        var state = comp.state;

        bool isProfessionTile = tileCounter % 2 == 0;

        if (isProfessionTile)
        {
            SpawnProfessions(tile, state, tileWorldZ);
            return;       
        }

        SpawnCollectibles(tile, state, tileWorldZ);
        SpawnBoost(tile, state, tileWorldZ);
        SpawnPowerUp(tile, state, tileWorldZ);
        SpawnDanger(tile, state, tileWorldZ);
    }
   
    private void SpawnProfessions(Transform tile, TileLaneState state, float tileWorldZ)
    {
        for (int i = tile.childCount - 1; i >= 0; i--)
        {
            Transform child = tile.GetChild(i);
            if (child != null && child.GetComponent<ProfessionObject>() != null)
            {
                if (SimplePool.Instance != null && child.GetComponent<PooledObject>() != null)
                    SimplePool.Instance.Despawn(child.gameObject);
                else
                    Destroy(child.gameObject);
            }
        }
        var profs = professionSpawner.GetTwoProfessionRequests(tile);

        if (profs != null && profs.Count >= 2)
        {
            state.laneOccupied[0] = false;
            state.laneOccupied[laneCount - 1] = false;

            SpawnAtLane(tile, state, profs[0], 0);
            SpawnAtLane(tile, state, profs[1], laneCount - 1);

            state.Occupy(0);
            state.Occupy(laneCount - 1);

            float profZ1 = tileWorldZ + profs[0].zOffset;
            float profZ2 = tileWorldZ + profs[1].zOffset;

            lastProfessionZ = profZ1;
            professionZPositions.Add(profZ1);
            professionZPositions.Add(profZ2);

            StartCoroutine(DelayedChoice(0, laneCount - 1));
        }
    }
    public void ResetGlobalZ()
    {
        professionZPositions.Clear();
        lastProfessionZ = -999f;
        lastDangerZ = -999f;
        tileCounter = 0;
    }

    private IEnumerator DelayedChoice(int left, int right)
    {
        yield return null;
        SlowMoController.Instance?.TriggerChoice(left, right);
    }

    private void SpawnAtLane(Transform tile, TileLaneState state, SpawnRequest req, int lane)
    {
        if (req == null || req.prefab == null) return;

        Vector3 pos = tile.TransformPoint(new Vector3(
            LaneToX(lane),
            req.height,
            req.zOffset
        ));

        GameObject obj;
        if (SimplePool.Instance != null)
        {
            obj = SimplePool.Instance.Spawn(req.prefab, pos, Quaternion.identity, tile);
        }
        else
        {
            obj = Instantiate(req.prefab, pos, Quaternion.identity, tile);
        }

        state.OccupyAtZ(lane, req.zOffset);

        req.onSpawn?.Invoke(obj);
    }
    private int FindFreeLane(TileLaneState state, float zOffset = 0f, float minZDistance = 2f)
    {
        List<int> free = new();
        for (int i = 0; i < laneCount; i++)
        {
            if (state.IsFreeAtZ(i, zOffset, minZDistance))
                free.Add(i);
        }

        return free.Count == 0 ? -1 : free[Random.Range(0, free.Count)];
    }

    private int FindFreeLaneExceptCenter(TileLaneState state, float zOffset = 0f, float minZDistance = 2f)
    {
        List<int> free = new();
        for (int i = 0; i < laneCount; i++)
        {
            if (i == laneCount / 2) continue;
            if (state.IsFreeAtZ(i, zOffset, minZDistance))
                free.Add(i);
        }
        return free.Count == 0 ? -1 : free[Random.Range(0, free.Count)];
    }
    private void SpawnDanger(Transform tile, TileLaneState state, float tileWorldZ)
    {
        if (IsNearProfession(tileWorldZ)) return;

        if ((tileCounter + 1) % 2 == 0) return;   
        if ((tileCounter - 1) % 2 == 0 && tileCounter > 1) return;   

        var requests = dangerSpawner.GetRequests(tile);
        if (requests == null || requests.Count == 0) return;

        int count = 0;

        foreach (var req in requests)
        {
            if (count >= maxDangersPerTile) break;

            float worldZ = tileWorldZ + req.zOffset;

            if (Mathf.Abs(worldZ - lastDangerZ) < dangerCooldown) continue;

            int lane = FindFreeLaneExceptCenter(state);
            if (lane == -1) break;

            SpawnAtLane(tile, state, req, lane);
            lastDangerZ = worldZ;
            count++;
        }
    }

    private void SpawnCollectibles(Transform tile, TileLaneState state, float tileWorldZ)
    {
        var requests = collectibleSpawner.GetRequests(tile);
        SpawnRequests(tile, state, requests, maxCollectiblesPerTile, true);   
    }

    private void SpawnBoost(Transform tile, TileLaneState state, float tileWorldZ)
    {
        var requests = boostSpawner.GetRequests(tile);
        SpawnRequests(tile, state, requests, maxBoostsPerTile, true);   
    }

    private void SpawnPowerUp(Transform tile, TileLaneState state, float tileWorldZ)
    {
        var requests = powerUpSpawner.GetRequests(tile);
        SpawnRequests(tile, state, requests, maxPowerUpsPerTile, true);   
    }

    private void SpawnRequests(Transform tile, TileLaneState state, List<SpawnRequest> reqs, int max, bool allowCenter = true)
    {
        if (reqs == null || reqs.Count == 0) return;

        int count = 0;

        foreach (var req in reqs)
        {
            if (count >= max) break;

            int lane;
            if (allowCenter)
            {
                lane = FindFreeLane(state, req.zOffset, 2f);
            }
            else
            {
                lane = FindFreeLaneExceptCenter(state, req.zOffset, 2f);
            }

            if (lane == -1) continue;         

            SpawnAtLane(tile, state, req, lane);
            count++;
        }
    }

    private float LaneToX(int lane)
    {
        float center = (laneCount - 1) * 0.5f;
        return (lane - center) * laneDistance;
    }
}