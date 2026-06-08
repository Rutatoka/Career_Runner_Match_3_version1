using UnityEngine;
using System.Collections;

public class BoostZone : MonoBehaviour
{
    public float speedMultiplier = 2f;
    public float scoreMultiplier = 2f;
    public float duration = 2f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Boost triggered! Speed multiplier: {speedMultiplier}, Duration: {duration}");
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        Debug.Log($"BoostZone triggered by {other.name}");
        triggered = true;

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log($"Applying boost: speed x{speedMultiplier} for {duration}s");
            StartCoroutine(ApplySpeedBoost(player));
        }

        ScoreManager.Instance?.SetBaseMultiplier(scoreMultiplier);
        Invoke(nameof(ResetScoreMultiplier), duration);

        Despawn();
    }

    private IEnumerator ApplySpeedBoost(PlayerController player)
    {
        float originalSpeed = player.forwardSpeed;
        player.forwardSpeed *= speedMultiplier;
        Debug.Log($"Speed boosted from {originalSpeed} to {player.forwardSpeed}");

        yield return new WaitForSeconds(duration);

        player.forwardSpeed = originalSpeed;
        Debug.Log($"Speed restored to {player.forwardSpeed}");
    }

    private void ResetScoreMultiplier()
    {
        ScoreManager.Instance?.ResetBaseMultiplier();
    }

    private void Despawn()
    {
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && SimplePool.Instance != null)
            SimplePool.Instance.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}