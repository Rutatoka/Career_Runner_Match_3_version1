using UnityEngine;
using System.Collections;

public class DangerObject : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowMultiplier = 0.5f;   // во сколько раз замедлить игрока
    public float slowDuration = 2f;       // сколько секунд длится замедление

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            StartCoroutine(ApplySlow(player));
        }
    }

    private IEnumerator ApplySlow(PlayerController player)
    {
        float originalSpeed = player.forwardSpeed;
        player.forwardSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        player.forwardSpeed = originalSpeed;
    }
}
