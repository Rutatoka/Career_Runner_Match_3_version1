using UnityEngine;
using System.Collections.Generic;

public class DangerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    public List<ObstacleEntry> obstacles = new();
    public float spawnChance = 0.4f;
    public float minZ = 10f;
    public float maxZ = 18f;

    public List<SpawnRequest> GetRequests(Transform tile)
    {
        var list = new List<SpawnRequest>();
        if (obstacles.Count == 0) return list;
        if (Random.value > spawnChance) return list;

        var entry = Choose();
        if (entry == null) return list;

        float z = Random.Range(minZ, maxZ);

        list.Add(new SpawnRequest
        {
            prefab = entry.prefab,
            height = 0f,
            zOffset = z
        });

        return list;
    }

    private ObstacleEntry Choose()
    {
        float total = 0f;
        foreach (var e in obstacles)
            total += e.weight;

        float r = Random.value * total;

        foreach (var e in obstacles)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return obstacles[0];
    }
}
