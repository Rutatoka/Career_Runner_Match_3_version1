// SimplePool.cs
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

public class SimplePool : MonoBehaviour
{
    private static SimplePool _instance;
    public static SimplePool Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SimplePool");
                _instance = go.AddComponent<SimplePool>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    private readonly Dictionary<GameObject, Transform> poolParents = new();

    // -----------------------
    // Spawn
    // -----------------------
    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (prefab == null) return null;

        EnsurePool(prefab);

        if (!pools.TryGetValue(prefab, out var queue) || queue.Count == 0)
        {
            var obj = Instantiate(prefab, pos, rot, parent);
            EnsurePooledObjectComponent(obj, prefab);
            obj.SetActive(true);
            CallOnSpawned(obj);
            return obj;
        }

        var pooled = queue.Dequeue();
        if (pooled == null)
        {
            // если в очереди оказалс€ уничтоженный объект Ч пробуем снова
            return Spawn(prefab, pos, rot, parent);
        }

        pooled.transform.SetParent(parent);
        pooled.transform.position = pos;
        pooled.transform.rotation = rot;
        pooled.SetActive(true);
        CallOnSpawned(pooled);
        return pooled;
    }

    // -----------------------
    // Despawn (перегрузки)
    // -----------------------

    /// <summary>
    /// ќригинальна€ верси€: возвращает объект в пул по ключу prefab.
    /// </summary>
    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null) return;

        instance.SetActive(false);
        EnsurePool(prefab);

        // ѕеремещаем в контейнер пула
        instance.transform.SetParent(poolParents[prefab], worldPositionStays: false);

        pools[prefab].Enqueue(instance);
        CallOnDespawned(instance);
    }

    /// <summary>
    /// ”добна€ перегрузка: определ€ет ключ по PooledObject.originalPrefab.
    /// ≈сли ключ не найден Ч делает безопасный fallback (удал€ет объект и логирует).
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        var pooledComp = instance.GetComponent<PooledObject>();
        GameObject key = pooledComp != null && pooledComp.originalPrefab != null ? pooledComp.originalPrefab : null;

        if (key == null)
        {
            Debug.LogWarning("SimplePool.Despawn(instance): original prefab not found on instance. Destroying instance as fallback.");
            instance.SetActive(false);
            Destroy(instance);
            return;
        }

        Despawn(key, instance);
    }

    // -----------------------
    // Preload / Clear
    // -----------------------
    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;
        EnsurePool(prefab);

        var q = pools[prefab];
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, poolParents[prefab]);
            EnsurePooledObjectComponent(obj, prefab);
            obj.SetActive(false);
            q.Enqueue(obj);
        }
    }

    public void ClearPool(GameObject prefab)
    {
        if (prefab == null) return;
        if (pools.TryGetValue(prefab, out var q))
        {
            while (q.Count > 0)
            {
                var obj = q.Dequeue();
                if (obj != null) Destroy(obj);
            }
            pools.Remove(prefab);
        }

        if (poolParents.TryGetValue(prefab, out var parent))
        {
            if (parent != null) Destroy(parent.gameObject);
            poolParents.Remove(prefab);
        }
    }

    public void ClearAll()
    {
        foreach (var kv in pools)
        {
            var q = kv.Value;
            while (q.Count > 0)
            {
                var obj = q.Dequeue();
                if (obj != null) Destroy(obj);
            }
        }
        pools.Clear();

        foreach (var kv in poolParents)
        {
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        }
        poolParents.Clear();
    }

    // -----------------------
    // Helpers
    // -----------------------
    private void EnsurePool(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }

        if (!poolParents.ContainsKey(prefab))
        {
            var parentGO = new GameObject($"Pool_{prefab.name}");
            parentGO.transform.SetParent(this.transform);
            poolParents[prefab] = parentGO.transform;
        }
    }

    private void EnsurePooledObjectComponent(GameObject obj, GameObject prefabKey)
    {
        if (obj == null) return;
        var pooledComp = obj.GetComponent<PooledObject>();
        if (pooledComp == null) pooledComp = obj.AddComponent<PooledObject>();
        pooledComp.originalPrefab = prefabKey;
    }

    private void CallOnSpawned(GameObject obj)
    {
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable != null) poolable.OnSpawned();

        var pooledObj = obj.GetComponent<PooledObject>();
        pooledObj?.OnSpawned();
    }

    private void CallOnDespawned(GameObject obj)
    {
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable != null) poolable.OnDespawned();

        var pooledObj = obj.GetComponent<PooledObject>();
        pooledObj?.OnDespawned();
    }

    private void OnDestroy()
    {
        ClearAll();
        _instance = null;
    }
}
