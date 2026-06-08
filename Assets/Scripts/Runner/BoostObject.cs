using UnityEngine;
using System.Collections;
public enum BoostType
{
    SpeedPad,
    JumpPad,
    DashRing
}

public class BoostObject : MonoBehaviour
{
    [Header("Boost Settings")]
    public BoostType boostType = BoostType.SpeedPad;

    public float speedMultiplier = 2f;
    public float boostDuration = 1.2f;

    public float jumpForce = 12f;
    public float dashForce = 20f;

    [Header("Visuals")]
    public ParticleSystem activateEffect;
    public AudioClip activateSound;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (boostType)
        {
            case BoostType.SpeedPad:
                StartCoroutine(ApplySpeedBoost(player));
                break;

            case BoostType.JumpPad:
                ApplyJump(player);
                break;

            case BoostType.DashRing:
                ApplyDash(player);
                break;
        }

        PlayEffects();
        DestroyOrDespawn();
    }

    private IEnumerator ApplySpeedBoost(PlayerController player)
    {
        player.ApplyBoost(speedMultiplier, boostDuration);
        yield return null;
    }

    private void ApplyJump(PlayerController player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void ApplyDash(PlayerController player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, dashForce);
    }

    private void PlayEffects()
    {
        if (activateEffect != null)
            Instantiate(activateEffect, transform.position, Quaternion.identity);

        if (activateSound != null)
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
    }

    private void DestroyOrDespawn()
    {
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && SimplePool.Instance != null)
            SimplePool.Instance.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}
