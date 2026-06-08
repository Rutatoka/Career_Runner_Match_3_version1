using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject tilePrefab;
    public Transform player;
    public TileObjectSpawner tileObjectSpawner;
    [Header("Layout")]
    [Min(1)] public int tilesOnScreen = 6;
    public float tileLength = 10f;
    public float startZ = 0f;
    public float spawnAheadDistance = 20f;
    private EnvironmentDecorator environmentDecorator;
    private float spawnZ;
    private readonly Queue<GameObject> tiles = new Queue<GameObject>();
    private bool initialized;
    private GroundGlowController groundGlowController;
    private TrackGenerator trackGenerator; // ? добавлено

    private void OnEnable()
    {
        if (!initialized) Initialize();
    }

    private void OnDisable()
    {
        // ClearAllTiles();  ❌ УБРАТЬ
        initialized = false;
    }


    private void Initialize()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("TileSpawner: tilePrefab is not assigned.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("TileSpawner: player reference is not assigned.");
            enabled = false;
            return;
        }

        // ищем TrackGenerator в сцене
        trackGenerator = FindObjectOfType<TrackGenerator>();
        environmentDecorator = FindObjectOfType<EnvironmentDecorator>();
        groundGlowController = FindObjectOfType<GroundGlowController>();
        spawnZ = startZ;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c != null) Destroy(c.gameObject);
        }

        SpawnInitialTiles();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || player == null) return;

        float threshold = spawnZ - tilesOnScreen * tileLength;
        if (player.position.z - spawnAheadDistance > threshold)
        {
            SpawnTile();
            DeleteOldestTile();
        }
    }

    public void SpawnInitialTiles()
    {
        for (int i = 0; i < Mathf.Max(1, tilesOnScreen); i++)
            SpawnTile();
    }

    public void SpawnTile()
    {
       // FindObjectOfType<TileObjectSpawner>()?.ResetGlobalZ();

        GameObject tileInstance = null;
        Vector3 spawnPos = Vector3.forward * spawnZ;
        Quaternion rot = Quaternion.identity;

        if (SimplePool.Instance != null)
            tileInstance = SimplePool.Instance.Spawn(tilePrefab, spawnPos, rot, transform);

        if (tileInstance == null)
            tileInstance = Instantiate(tilePrefab, spawnPos, rot, transform);

        tileInstance.transform.SetParent(transform, worldPositionStays: false);
        tileInstance.transform.localPosition = Vector3.forward * spawnZ;

        // ВАЖНО: сохраняем текущий Z
        float tileWorldZ = spawnZ;

        tiles.Enqueue(tileInstance);
        spawnZ += Mathf.Max(0.01f, tileLength);

        if (trackGenerator != null)
            trackGenerator.PopulateTile(tileInstance, tileWorldZ);

        environmentDecorator?.DecorateTile(tileInstance, tileWorldZ);
        groundGlowController?.RegisterTile(tileInstance);
    }


    private void DeleteOldestTile()
    {
        if (tiles.Count == 0) return;
        var old = tiles.Dequeue();
        if (old == null) return;

        var pooled = old.GetComponent<PooledObject>();
        if (pooled != null && pooled.originalPrefab != null && SimplePool.Instance != null)
        {
            SimplePool.Instance.Despawn(old);
        }
        else
        {
            environmentDecorator?.CleanupTile(old);
            groundGlowController?.CleanupTile(old);
            Destroy(old);
        }
    }

    public void ClearAllTiles()
    {
        while (tiles.Count > 0)
        {
            var t = tiles.Dequeue();
            if (t == null) continue;
            var pooled = t.GetComponent<PooledObject>();
            if (pooled != null && pooled.originalPrefab != null && SimplePool.Instance != null)
                SimplePool.Instance.Despawn(t);
            else
                Destroy(t);
        }
    }

    public void ResetSpawner()
    {
        ClearAllTiles();
        spawnZ = startZ;
        SpawnInitialTiles();
    }

}
