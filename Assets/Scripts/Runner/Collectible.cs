using UnityEngine;

public class Collectible : MonoBehaviour, IPoolable
{
    [Header("Optional Data")]
    public CollectibleData data; // можно не заполнять, если это просто монета

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // 1) Уведомляем CollectibleManager
        CollectibleManager.Instance?.AddCoins(1);

        // 2) Комбо
        ComboSystem.Instance?.AddCombo();

        // 3) Эффекты (если есть)
        if (data != null && data.isRare)
        {
            // можно добавить редкий эффект
        }

        // 4) Удаляем объект или возвращаем в пул
        Despawn();
    }
    public void OnSpawned()
    {
        collected = false;

        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    public void OnDespawned()
    {
        collected = false;
    }

    private void Despawn()
    {
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && SimplePool.Instance != null)
        {
            SimplePool.Instance.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
