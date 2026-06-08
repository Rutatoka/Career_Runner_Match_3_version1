using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public class DecisionTimer : MonoBehaviour
{
    [Header("UI")]
 //   public Slider timerSlider;
    public TextMeshProUGUI progressText;

    [Header("Settings")]
    public float maxTime = 10f;
    public bool startOnEnable = false;

    [Header("Events")]
    public FloatEvent onTick;        // passes normalized remaining (0..1)
    public UnityEvent onExpired;     // called when timer reaches zero

    private float currentTime;
    private bool isActive;
    private bool isPaused;

    public void Init(int totalSeconds)
    {
        maxTime = Mathf.Max(0.01f, totalSeconds);
        ResetTimer();
        UpdateUI();
    }

    public void StartTimer(int currentIndex = 0, int totalCount = 1)
    {
        if (progressText != null)
            progressText.text = $"{currentIndex}/{totalCount}";

        if (maxTime <= 0f) maxTime = 1f;
        currentTime = maxTime;
        isActive = true;
        isPaused = false;
        UpdateUI();
    }

    public void StartTimerWithSeconds(float seconds, int currentIndex = 0, int totalCount = 1)
    {
        maxTime = Mathf.Max(0.01f, seconds);
        StartTimer(currentIndex, totalCount);
    }

    public void StopTimer()
    {
        isActive = false;
        isPaused = false;
    }

    public void Pause()
    {
        if (!isActive) return;
        isPaused = true;
    }

    public void Resume()
    {
        if (!isActive) return;
        isPaused = false;
    }

    public void ResetTimer()
    {
        currentTime = maxTime;
        isActive = false;
        isPaused = false;
        UpdateUI();
    }

    public bool IsRunning => isActive && !isPaused;
    public float RemainingSeconds => currentTime;
    public float Normalized => maxTime > 0f ? Mathf.Clamp01(currentTime / maxTime) : 0f;

    private void OnEnable()
    {
        if (startOnEnable)
        {
            ResetTimer();
            isActive = true;
            isPaused = false;
        }
    }

    private void Update()
    {
        if (!isActive || isPaused) return;

        currentTime -= Time.unscaledDeltaTime;
        currentTime = Mathf.Max(0f, currentTime);

        UpdateUI();

        onTick?.Invoke(Normalized);

        if (currentTime <= 0f)
        {
            isActive = false;
            isPaused = false;
            HandleExpired();
        }
    }

    private void UpdateUI()
    {
        //if (timerSlider != null)
        //{
        //    timerSlider.value = maxTime > 0f ? Mathf.Clamp01(currentTime / maxTime) : 0f;
        //}

        if (progressText != null && string.IsNullOrEmpty(progressText.text))
        {
            progressText.text = $"{Mathf.CeilToInt(currentTime)}s";
        }
    }

    private void HandleExpired()
    {
        onExpired?.Invoke();
    }
}
