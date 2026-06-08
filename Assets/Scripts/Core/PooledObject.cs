// PooledObject.cs
using UnityEngine;

/// <summary>
/// Помечает инстанс как принадлежащий пулу и хранит ссылку на оригинальный prefab.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [Tooltip("Original prefab reference used as pool key")]
    public GameObject originalPrefab;

    public virtual void OnSpawned()
    {
        // override to reset state
    }

    public virtual void OnDespawned()
    {
        // override to cleanup
    }
}
