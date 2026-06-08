//using UnityEngine;
//using System.Collections.Generic;

//public class ProfessionSpawner : MonoBehaviour
//{
//    [System.Serializable]
//    public class ProfessionEntry
//    {
//        public ProfessionType type;
//        public GameObject prefab;
//        public ProfessionObjectData objectData;
//        public float weight = 1f;
//    }

//    public List<ProfessionEntry> professionObjects = new();
//    public float spawnHeight = 1f;
//    public float minZ = 4f;
//    public float maxZ = 12f;
//    public float professionZ = 3f;

//    private Queue<ProfessionEntry> fairQueue = new Queue<ProfessionEntry>();
//    private Dictionary<ProfessionType, int> spawnStats = new();

//    private void Awake()
//    {
//        InitStats();
//    }

//    private void Start()
//    {
//        RefillQueue();
//    }

//    private void InitStats()
//    {
//        spawnStats = new Dictionary<ProfessionType, int>();
//        foreach (var entry in professionObjects)
//        {
//            if (!spawnStats.ContainsKey(entry.type))
//                spawnStats.Add(entry.type, 0);
//        }
//    }

//    private void RefillQueue()
//    {
//        fairQueue.Clear();

//        // Создаём список всех профессий
//        List<ProfessionEntry> temp = new List<ProfessionEntry>(professionObjects);

//        // Перемешиваем
//        for (int i = 0; i < temp.Count; i++)
//        {
//            var t = temp[i];
//            int r = Random.Range(i, temp.Count);
//            temp[i] = temp[r];
//            temp[r] = t;
//        }

//        // Добавляем в очередь
//        foreach (var e in temp)
//            fairQueue.Enqueue(e);

//        Debug.Log($"[ProfessionSpawner] Queue refilled with {fairQueue.Count} entries");
//    }

//    public List<SpawnRequest> GetRequests(Transform tile)
//    {
//        return new List<SpawnRequest>(); // пусто
//    }

//    public List<SpawnRequest> GetTwoProfessionRequests(Transform tile)
//    {
//        List<SpawnRequest> list = new();
//        if (professionObjects.Count < 2)
//        {
//            Debug.LogWarning("[ProfessionSpawner] Not enough professions!");
//            return list;
//        }

//        // Проверяем и пополняем очередь если нужно
//        if (fairQueue.Count < 2)
//        {
//            Debug.Log("[ProfessionSpawner] Queue depleted, refilling...");
//            RefillQueue();
//        }

//        // Берём первую профессию
//        ProfessionEntry first = fairQueue.Dequeue();

//        // Берём вторую, отличную от первой
//        ProfessionEntry second;

//        // Если в очереди есть ещё элементы
//        if (fairQueue.Count > 0)
//        {
//            second = fairQueue.Dequeue();

//            // Если вторая такая же как первая, пробуем взять другую
//            if (second.type == first.type && fairQueue.Count > 0)
//            {
//                // Возвращаем вторую обратно в очередь
//                fairQueue.Enqueue(second);
//                // Берём следующую
//                second = fairQueue.Dequeue();
//            }
//        }
//        else
//        {
//            // Очередь пуста - пополняем и берём отличную от первой
//            RefillQueue();
//            second = fairQueue.Dequeue();

//            // Если опять такая же, берём следующую
//            if (second.type == first.type && fairQueue.Count > 0)
//            {
//                fairQueue.Enqueue(second);
//                second = fairQueue.Dequeue();
//            }
//        }

//        float z = professionZ;
//        list.Add(MakeRequest(first, z));
//        list.Add(MakeRequest(second, z));

//        // БЕЗОПАСНО обновляем статистику
//        UpdateSpawnStat(first.type);
//        UpdateSpawnStat(second.type);

//        Debug.Log($"[ProfessionSpawner] Spawned: {first.type} and {second.type}. Queue: {fairQueue.Count}");

//        return list;
//    }

//    // Безопасное обновление статистики
//    private void UpdateSpawnStat(ProfessionType type)
//    {
//        if (spawnStats.ContainsKey(type))
//        {
//            spawnStats[type]++;
//        }
//        else
//        {
//            spawnStats.Add(type, 1);
//        }
//    }

//    private SpawnRequest MakeRequest(ProfessionEntry entry, float z)
//    {
//        return new SpawnRequest
//        {
//            prefab = entry.prefab,
//            height = spawnHeight,
//            zOffset = z,
//            onSpawn = (obj) =>
//            {
//                ProfessionObject profObj = obj.GetComponent<ProfessionObject>();
//                if (profObj != null && entry.objectData != null)
//                {
//                    profObj.data = entry.objectData;
//                }
//            }
//        };
//    }

//    public void ResetSpawnStats()
//    {
//        InitStats();
//        RefillQueue();
//        Debug.Log("[ProfessionSpawner] Stats and queue reset");
//    }
//}
using UnityEngine;
using System.Collections.Generic;

public class ProfessionSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ProfessionEntry
    {
        public ProfessionType type;
        public GameObject prefab;
        public ProfessionObjectData objectData;
        public float weight = 1f;
    }

    public List<ProfessionEntry> professionObjects = new();
    public float spawnHeight = 1f;
    public float minZ = 4f;
    public float maxZ = 12f;
    public float professionZ = 8f;

    private Queue<ProfessionEntry> fairQueue = new Queue<ProfessionEntry>();
    private Dictionary<ProfessionType, int> spawnStats = new();

    private void Awake()
    {
      //  Debug.Log($"[ProfessionSpawner] Awake. Profession objects count: {professionObjects.Count}");

        if (professionObjects.Count == 0)
        {
            Debug.LogError("[ProfessionSpawner] NO PROFESSION OBJECTS ASSIGNED IN INSPECTOR!");
        }

        foreach (var obj in professionObjects)
        {
      //      Debug.Log($"[ProfessionSpawner] Profession: {obj.type}, Prefab: {(obj.prefab != null ? obj.prefab.name : "NULL")}, ObjectData: {(obj.objectData != null ? obj.objectData.name : "NULL")}");
        }

        InitStats();
    }

    private void Start()
    {
        RefillQueue();
    }

    private void InitStats()
    {
        spawnStats = new Dictionary<ProfessionType, int>();
        foreach (var entry in professionObjects)
        {
            if (!spawnStats.ContainsKey(entry.type))
                spawnStats.Add(entry.type, 0);
        }
    }

    private void RefillQueue()
    {
        fairQueue.Clear();

        if (professionObjects.Count == 0)
        {
            Debug.LogError("[ProfessionSpawner] Cannot refill queue - no profession objects!");
            return;
        }

        List<ProfessionEntry> temp = new List<ProfessionEntry>(professionObjects);

        // Перемешиваем
        for (int i = 0; i < temp.Count; i++)
        {
            var t = temp[i];
            int r = Random.Range(i, temp.Count);
            temp[i] = temp[r];
            temp[r] = t;
        }

        // Добавляем в очередь
        foreach (var e in temp)
        {
            if (e.prefab != null)
            {
                fairQueue.Enqueue(e);
            }
            else
            {
                Debug.LogError($"[ProfessionSpawner] Skipping profession {e.type} - prefab is NULL!");
            }
        }

    //    Debug.Log($"[ProfessionSpawner] Queue refilled with {fairQueue.Count} entries");
    }

    public List<SpawnRequest> GetRequests(Transform tile)
    {
        return new List<SpawnRequest>(); // пусто
    }

    public List<SpawnRequest> GetTwoProfessionRequests(Transform tile)
    {
        List<SpawnRequest> list = new();

    //    Debug.Log($"[ProfessionSpawner] GetTwoProfessionRequests called. Objects count: {professionObjects.Count}, Queue size: {fairQueue.Count}");

        if (professionObjects.Count < 2)
        {
            Debug.LogError($"[ProfessionSpawner] Not enough professions! Need 2, have {professionObjects.Count}");
            return list;
        }

        // Проверяем и пополняем очередь если нужно
        if (fairQueue.Count < 2)
        {
        //    Debug.Log("[ProfessionSpawner] Queue depleted, refilling...");
            RefillQueue();
        }

        if (fairQueue.Count < 2)
        {
            Debug.LogError("[ProfessionSpawner] STILL not enough entries in queue after refill!");
            return list;
        }

        // Берём первую профессию
        ProfessionEntry first = fairQueue.Dequeue();
      //  Debug.Log($"[ProfessionSpawner] First profession: {first.type}, prefab: {(first.prefab != null ? first.prefab.name : "NULL")}");

        // Берём вторую, отличную от первой
        ProfessionEntry second = null;

        // Пробуем найти отличную профессию
        List<ProfessionEntry> tempList = new List<ProfessionEntry>();
        int attempts = 0;
        int maxAttempts = fairQueue.Count + 1;

        while (fairQueue.Count > 0 && attempts < maxAttempts)
        {
            var candidate = fairQueue.Dequeue();
            attempts++;

            if (candidate.type != first.type && second == null)
            {
                second = candidate;
               // Debug.Log($"[ProfessionSpawner] Second profession: {second.type}");
            }
            else
            {
                tempList.Add(candidate);
            }
        }

        // Возвращаем остальные в очередь
        foreach (var entry in tempList)
        {
            fairQueue.Enqueue(entry);
        }

        // Если не нашли отличную, берём любую
        if (second == null)
        {
            Debug.LogWarning("[ProfessionSpawner] Could not find different profession, taking same type");

            // Пополняем очередь и пробуем снова
            RefillQueue();

            if (fairQueue.Count > 0)
            {
                second = fairQueue.Dequeue();
            //    Debug.Log($"[ProfessionSpawner] Second profession (fallback): {second.type}");
            }
            else
            {
          //      Debug.LogError("[ProfessionSpawner] CRITICAL: No professions available!");
                return list;
            }
        }

        if (first.prefab == null || second.prefab == null)
        {
          //  Debug.LogError($"[ProfessionSpawner] NULL PREFAB! First: {(first.prefab != null ? "OK" : "NULL")}, Second: {(second.prefab != null ? "OK" : "NULL")}");
            return list;
        }

        float z = professionZ;
        list.Add(MakeRequest(first, z));
        list.Add(MakeRequest(second, z));

        UpdateSpawnStat(first.type);
        UpdateSpawnStat(second.type);

       // Debug.Log($"[ProfessionSpawner] SUCCESS: Spawned {first.type} and {second.type}. Remaining in queue: {fairQueue.Count}");

        // Выводим статистику
        foreach (var stat in spawnStats)
        {
           // Debug.Log($"[ProfessionSpawner] Stat: {stat.Key} = {stat.Value}");
        }

        return list;
    }

    private void UpdateSpawnStat(ProfessionType type)
    {
        if (spawnStats.ContainsKey(type))
        {
            spawnStats[type]++;
        }
        else
        {
            spawnStats.Add(type, 1);
        }
    }

    private SpawnRequest MakeRequest(ProfessionEntry entry, float z)
    {
        return new SpawnRequest
        {
            prefab = entry.prefab,
            height = spawnHeight,
            zOffset = z,
            onSpawn = (obj) =>
            {
                ProfessionObject profObj = obj.GetComponent<ProfessionObject>();
                if (profObj != null && entry.objectData != null)
                {
                    profObj.data = entry.objectData;
                }
            }
        };
    }

    public void ResetSpawnStats()
    {
        InitStats();
        RefillQueue();
      //  Debug.Log("[ProfessionSpawner] Stats and queue reset");
    }
}