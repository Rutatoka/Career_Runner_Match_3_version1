using UnityEngine;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PowerUpEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    public List<PowerUpEntry> powerUps = new();
    public float spawnChance = 0.15f;
    public float spawnHeight = 1f;
    public float minZ = 2f;
    public float maxZ = 8f;

    public List<SpawnRequest> GetRequests(Transform tile)
    {
        List<SpawnRequest> list = new();
        if (powerUps.Count == 0) return list;
        if (Random.value > spawnChance) return list;

        var entry = Choose();
        if (entry == null) return list;

        float z = Random.Range(minZ, maxZ);

        list.Add(new SpawnRequest
        {
            prefab = entry.prefab,
            height = spawnHeight,
            zOffset = z,
            onSpawn = (obj) =>
            {
                if (obj.GetComponent<PowerUpObject>() == null)
                    obj.AddComponent<PowerUpObject>();
            }
        });

        return list;
    }

    private PowerUpEntry Choose()
    {
        float total = 0f;
        foreach (var e in powerUps)
            total += e.weight;

        float r = Random.value * total;

        foreach (var e in powerUps)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return powerUps[0];
    }
}
