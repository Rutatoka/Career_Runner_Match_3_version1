using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Positioning")]
    public float height = 5f;
    public float forwardOffset = 6f;

    [Header("Smoothing")]
    public float smooth = 10f;
    public bool useSmoothDamp = true;

    [Header("X Offset")]
    public float maxXOffset = 2.5f;
    public float xOffsetMultiplier = 1f;

    private float velocityX = 0f;

    private void LateUpdate()
    {
        if (player == null) return;

        float desiredX = Mathf.Clamp(player.position.x * xOffsetMultiplier, -Mathf.Abs(maxXOffset), Mathf.Abs(maxXOffset));

        float newX;
        if (useSmoothDamp)
        {
            newX = Mathf.SmoothDamp(transform.position.x, desiredX, ref velocityX, 1f / Mathf.Max(0.0001f, smooth));
        }
        else
        {
            newX = Mathf.Lerp(transform.position.x, desiredX, smooth * Time.deltaTime);
        }

        Vector3 targetPos = new Vector3(newX, height, player.position.z - forwardOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Clamp01(smooth * Time.deltaTime));
    }

    private void OnValidate()
    {
        smooth = Mathf.Max(0.01f, smooth);
        maxXOffset = Mathf.Max(0f, maxXOffset);
        xOffsetMultiplier = Mathf.Max(0f, xOffsetMultiplier);
        forwardOffset = Mathf.Max(0f, forwardOffset);
    }
}
