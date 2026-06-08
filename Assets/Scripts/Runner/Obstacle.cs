using UnityEngine;

/// <summary>
/// Логика препятствия:
/// - реагирует на столкновение с игроком
/// - проверяет щит
/// - вызывает смерть игрока
/// - поддерживает разрушение
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Settings")]
    public bool destroyableWithShield = true;
    public bool destroyAfterHit = true;

    [Header("Effects")]
    public ParticleSystem destroyEffect;
    public AudioClip hitSound;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        HandleCollision();
    }

    private void HandleCollision()
    {
      

        // Если щита нет — смерть
        RunnerController.Instance?.KillPlayer();
    }

    private void DestroyObstacle()
    {
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

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
