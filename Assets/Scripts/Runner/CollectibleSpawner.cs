using UnityEngine;
using System.Collections.Generic;

public class CollectibleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    public List<CollectibleEntry> collectibles = new();
    public float spawnChance = 1f;
    public float spawnHeight = 1f;
    public float minZ = 4f;
    public float maxZ = 12f;
    public bool allowLinePattern = true;
    public int minLineCount = 2;
    public int maxLineCount = 4;
    public float lineSpacing = 2.3f;

    public List<SpawnRequest> GetRequests(Transform tile)
    {
        List<SpawnRequest> list = new();
        if (collectibles.Count == 0) return list;
        if (Random.value > spawnChance) return list;

        var entry = Choose();
        if (entry == null) return list;

        float z = Random.Range(minZ, maxZ);

        if (!allowLinePattern || Random.value < 0.5f)
        {
            list.Add(new SpawnRequest
            {
                prefab = entry.prefab,
                height = spawnHeight,
                zOffset = z,
                onSpawn = (obj) =>
                {
                    if (obj.GetComponent<Collectible>() == null)
                        obj.AddComponent<Collectible>();
                }
            });

            return list;
        }

        int count = Random.Range(minLineCount, maxLineCount + 1);

        for (int i = 0; i < count; i++)
        {
            float zOffset = z + i * lineSpacing;

            list.Add(new SpawnRequest
            {
                prefab = entry.prefab,
                height = spawnHeight,
                zOffset = zOffset,
                onSpawn = (obj) =>
                {
                    if (obj.GetComponent<Collectible>() == null)
                        obj.AddComponent<Collectible>();
                }
            });
        }

        return list;
    }

    private CollectibleEntry Choose()
    {
        float total = 0f;
        foreach (var e in collectibles)
            total += e.weight;

        float r = Random.value * total;

        foreach (var e in collectibles)
        {
            if (r < e.weight)
                return e;
            r -= e.weight;
        }

        return collectibles[0];
    }
}
