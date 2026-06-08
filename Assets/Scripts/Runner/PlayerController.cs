using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed;
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 15f;
    private float originalForwardSpeed;
    private Rigidbody rb;
    [Header("Lanes")]
    public int laneCount = 3;
    public int startLane = 1;

    [Header("Input")]
    public float swipeThreshold = 50f;

    [Header("References")]
    public CharacterModelController characterModel;

    public event Action<int> OnLaneChanged;
    public event Action<GameObject> OnCollectedGem;

    private int targetLane;
    private Vector2 startTouch;
    private bool touchActive;
    private bool inputEnabled = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetLane = Mathf.Clamp(startLane, 0, laneCount - 1);
        characterModel?.UpdateCharacter();
        originalForwardSpeed = forwardSpeed;
    }

    private void Update()
    {
        MoveForward();

        if (inputEnabled)
            HandleInput();

        MoveToLane();
    }
   // public float CurrentSpeed => RunnerController.Instance.currentSpeed;

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);


    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition;
            touchActive = true;
        }

        if (Input.GetMouseButtonUp(0) && touchActive)
        {
            Vector2 delta = (Vector2)Input.mousePosition - startTouch;
            ProcessSwipe(delta);
            touchActive = false;
        }
#else
        if (Input.touchCount == 0) return;

        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            startTouch = touch.position;
            touchActive = true;
        }
        else if (touch.phase == TouchPhase.Ended && touchActive)
        {
            Vector2 delta = touch.position - startTouch;
            ProcessSwipe(delta);
            touchActive = false;
        }
#endif
    }

    private void ProcessSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) < swipeThreshold) return;

        if (delta.x > 0) ChangeLane(1);
        else ChangeLane(-1);
    }

    private void ChangeLane(int direction)
    {
        int newLane = Mathf.Clamp(targetLane + direction, 0, laneCount - 1);
        if (newLane == targetLane) return;

        targetLane = newLane;
        OnLaneChanged?.Invoke(targetLane);
    }

    private void MoveToLane()
    {
        float center = (laneCount - 1) * 0.5f;
        float targetX = (targetLane - center) * laneDistance;

        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, laneChangeSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.CompareTag("Gem"))
        {
            var gem = other.GetComponent<Gem>();
            if (gem != null)
            {
                gem.Collect();
                OnCollectedGem?.Invoke(other.gameObject);
                return;
            }

            var g = other.GetComponentInChildren<Gem>();
            if (g != null)
            {
                g.Collect();
                OnCollectedGem?.Invoke(other.gameObject);
            }
        }
    }
    public void ApplyBoost(float multiplier, float duration)
    {
        forwardSpeed = originalForwardSpeed * multiplier; // Используем оригинал
      //  Debug.Log($"Speed boosted to {forwardSpeed}");

        Invoke(nameof(ResetSpeed), duration);
    }

    private void ResetSpeed()
    {
        forwardSpeed = originalForwardSpeed; // Возвращаем к оригиналу
     //   Debug.Log($"Speed restored to {forwardSpeed}");
    }
    public void ForceSetLane(int laneIndex)
    {
        targetLane = Mathf.Clamp(laneIndex, 0, laneCount - 1);
        OnLaneChanged?.Invoke(targetLane);
    }

    public void EnableInput(bool enabled)
    {
        inputEnabled = enabled;
    }
}
