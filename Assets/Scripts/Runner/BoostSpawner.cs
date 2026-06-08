using UnityEngine;
using System.Collections.Generic;

public class BoostSpawner : MonoBehaviour
{
    public GameObject boostPrefab;
    [Range(0f, 1f)] public float spawnChance = 0.5f;

    public float spawnHeight = 0.1f;
    public float minZ = 6f;
    public float maxZ = 14f;

    public List<SpawnRequest> GetRequests(Transform tile)
    {
        List<SpawnRequest> list = new();

        if (boostPrefab == null) return list;
        if (Random.value > spawnChance) return list;

        float z = Random.Range(minZ, maxZ);

        list.Add(new SpawnRequest
        {
            prefab = boostPrefab,
            height = spawnHeight,
            zOffset = z,
            onSpawn = (obj) =>
            {
                // Удаляем старый BoostZone, если есть
                var oldZone = obj.GetComponent<BoostZone>();
                if (oldZone != null)
                    DestroyImmediate(oldZone);

                // Добавляем BoostObject
                var boost = obj.GetComponent<BoostObject>();
                if (boost == null)
                    boost = obj.AddComponent<BoostObject>();

                // Настраиваем параметры
                boost.boostType = BoostType.SpeedPad;
                boost.speedMultiplier = 2f;
                boost.boostDuration = 1f; //
            }
        });

        return list;
    }
}
