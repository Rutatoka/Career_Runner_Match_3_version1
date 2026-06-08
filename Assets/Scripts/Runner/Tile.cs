using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Gem Spawn")]
    public GameObject gemPrefab;
    [Range(0f, 1f)] public float spawnChance = 0.7f;
    public float spawnOffsetZ = 5f;

    [Header("Lane Settings")]
    public int laneCount = 3;
    public float laneDistance = 2.5f;
    public float spawnHeight = 1f;


    public void TrySpawnGem()
    {
        if (gemPrefab == null) return;
        if (Random.value > spawnChance) return;

        float laneX = RandomLaneX();
        Vector3 localPos = new Vector3(laneX, spawnHeight, spawnOffsetZ);
        Vector3 worldPos = transform.TransformPoint(localPos);

        GameObject instance = null;
        if (SimplePool.Instance != null)
        {
            instance = SimplePool.Instance.Spawn(gemPrefab, worldPos, Quaternion.identity, transform);
        }

        if (instance == null)
        {
            instance = Instantiate(gemPrefab, worldPos, Quaternion.identity, transform);
        }

        if (instance != null)
        {
            try { instance.tag = "Gem"; } catch { }
            var col = instance.GetComponent<Collider>();
            if (col != null && !col.isTrigger) col.isTrigger = true;
        }
    }

    private float RandomLaneX()
    {
        if (laneCount <= 1) return 0f;
        int lane = Random.Range(0, laneCount);
        float center = (laneCount - 1) * 0.5f;
        return (lane - center) * laneDistance;
    }
}
