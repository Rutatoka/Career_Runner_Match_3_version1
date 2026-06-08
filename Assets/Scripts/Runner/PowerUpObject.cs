using UnityEngine;

/// <summary>
/// Компонент на объекте усилителя:
/// - хранит PowerUpItem
/// - реагирует на игрока
/// - активирует усилитель через PowerUpManager
/// - деспавнит объект
/// </summary>
public class PowerUpObject : MonoBehaviour
{
    [Header("Data")]
    public PowerUpItem item;

    [Header("Settings")]
    public bool autoColorize = true;
    public bool autoName = true;

    private bool collected = false;

    private void Start()
    {
        if (item == null)
        {
            Debug.LogWarning("PowerUpObject: item is NULL!");
            return;
        }

        // Красим объект в цвет усилителя
        if (autoColorize)
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material.color = item.uiColor;
        }

        // Переименовываем объект
        if (autoName)
            gameObject.name = "PowerUp_" + item.type;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        HandlePickup();
    }

    private void HandlePickup()
    {
        if (item == null)
        {
            DestroyOrDespawn();
            return;
        }

        // Активируем усилитель
        PowerUpManager.Instance?.ActivatePowerUp(item);

        // Можно добавить звук/эффект
        // AudioManager.Instance?.Play("PowerUp");

        DestroyOrDespawn();
    }

    private void DestroyOrDespawn()
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
