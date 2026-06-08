using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DailyTaskUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;
    public Button claimButton;
    public GameObject completedBadge;
    public TextMeshProUGUI claimButtonText;

    private DailyTask currentTask;
    private Action<DailyTask> onClaimed;

    private void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimClicked);
    }

    private void OnDestroy()
    {
        if (claimButton != null)
            claimButton.onClick.RemoveListener(OnClaimClicked);
    }

    public void Setup(DailyTask task, Action<DailyTask> onClaimedCallback)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        currentTask = task;
        onClaimed = onClaimedCallback;

        if (titleText != null) titleText.text = currentTask.title;
        if (descriptionText != null) descriptionText.text = currentTask.description;

        LoadPersistedState();
        RefreshAll();
    }

    private void LoadPersistedState()
    {
        if (currentTask == null) return;
        string progressKey = $"daily_{currentTask.id}_progress";
        string completedKey = $"daily_{currentTask.id}_completed";

        if (PlayerPrefs.HasKey(progressKey))
            currentTask.currentProgress = PlayerPrefs.GetInt(progressKey, currentTask.currentProgress);

        if (PlayerPrefs.HasKey(completedKey))
            currentTask.isCompleted = PlayerPrefs.GetInt(completedKey, 0) == 1;
    }

    private void PersistState()
    {
        if (currentTask == null) return;
        string progressKey = $"daily_{currentTask.id}_progress";
        string completedKey = $"daily_{currentTask.id}_completed";

        PlayerPrefs.SetInt(progressKey, currentTask.currentProgress);
        PlayerPrefs.SetInt(completedKey, currentTask.isCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RefreshAll()
    {
        if (currentTask == null) return;
        UpdateUI();
    }

    public void RefreshProgress(int newProgress)
    {
        if (currentTask == null) return;
        currentTask.currentProgress = Mathf.Clamp(newProgress, 0, currentTask.targetProgress);
        if (currentTask.currentProgress >= currentTask.targetProgress) currentTask.isCompleted = true;
        PersistState();
        UpdateUI();
    }

    public void UpdateProgress(int amount)
    {
        if (currentTask == null || currentTask.isCompleted) return;
        currentTask.AddProgress(amount);
        PersistState();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentTask == null) return;

        if (progressText != null)
            progressText.text = $"{currentTask.currentProgress}/{currentTask.targetProgress}";

        if (progressSlider != null)
        {
            progressSlider.maxValue = Mathf.Max(1, currentTask.targetProgress);
            progressSlider.value = Mathf.Clamp(currentTask.currentProgress, 0, currentTask.targetProgress);
        }

        bool complete = currentTask.IsComplete;

        if (completedBadge != null)
            completedBadge.SetActive(currentTask.isCompleted);

        if (claimButton != null)
            claimButton.gameObject.SetActive(!currentTask.isCompleted);

        if (claimButtonText != null)
        {
            if (currentTask.isCompleted)
            {
                claimButtonText.text = "Получено";
                claimButton.interactable = false;
            }
            else if (complete)
            {
                claimButtonText.text = $"Получить {currentTask.reward}";
                claimButton.interactable = true;
            }
            else
            {
                claimButtonText.text = $"Награда: {currentTask.reward}";
                claimButton.interactable = false;
            }
        }
    }

    private void OnClaimClicked()
    {
        if (currentTask == null) return;
        if (currentTask.isCompleted) return;
        if (currentTask.currentProgress < currentTask.targetProgress) return;

        currentTask.MarkCompleted();
        PersistState();
        UpdateUI();

        try
        {
            onClaimed?.Invoke(currentTask);
        }
        catch (Exception e)
        {
            Debug.LogError($"DailyTaskUI: exception in onClaimed callback: {e}");
        }
    }
}
