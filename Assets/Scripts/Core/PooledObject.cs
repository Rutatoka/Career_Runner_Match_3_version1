using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector]
    public GameObject originalPrefab; // Ссылка на префаб-родитель, чтобы пул знал, куда возвращать объект

    // Эти методы можно оставить пустыми, но они пригодятся, если захочешь сбрасывать состояние объекта
    public void OnSpawned() { }
    public void OnDespawned() { }
}