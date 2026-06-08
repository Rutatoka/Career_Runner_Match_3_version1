using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public static PlayerJump Instance;

    public float jumpForce = 7f;
    public float boostedJumpForce = 12f;

    private bool boostActive = false;
    private Rigidbody rb;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
    }

    public void Jump()
    {
        if (rb == null) return;

        float force = boostActive ? boostedJumpForce : jumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, force, rb.linearVelocity.z);
    }

    public void EnableJumpBoost(float duration)
    {
        boostActive = true;
        StopAllCoroutines();
        StartCoroutine(BoostTimer(duration));
    }

    public void DisableJumpBoost()
    {
        boostActive = false;
    }

    private System.Collections.IEnumerator BoostTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        DisableJumpBoost();
    }
}
