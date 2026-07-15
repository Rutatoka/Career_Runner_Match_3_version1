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

        // ВЫРЕЗАЛИ LoadPersistedState()! 
        // Менеджер уже загрузил все актуальные данные при старте сцены.

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (currentTask == null) return;
        UpdateUI();
    }

    // Оставляем методы изменения прогресса на случай, если ты дергаешь их напрямую откуда-то еще
    public void RefreshProgress(int newProgress)
    {
        if (currentTask == null) return;
        currentTask.currentProgress = Mathf.Clamp(newProgress, 0, currentTask.targetProgress);

        PlayerPrefs.SetInt($"daily_{currentTask.id}_progress", currentTask.currentProgress);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void UpdateProgress(int amount)
    {
        if (currentTask == null || currentTask.isCompleted) return;
        currentTask.AddProgress(amount);

        PlayerPrefs.SetInt($"daily_{currentTask.id}_progress", currentTask.currentProgress);
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentTask == null) return;

        // Обновляем текст прогресса (например, 5/15)
        if (progressText != null)
            progressText.text = $"{currentTask.currentProgress}/{currentTask.targetProgress}";

        // Двигаем слайдер
        if (progressSlider != null)
        {
            progressSlider.maxValue = Mathf.Max(1, currentTask.targetProgress);
            progressSlider.value = Mathf.Clamp(currentTask.currentProgress, 0, currentTask.targetProgress);
        }

        bool isTargetReached = currentTask.IsComplete; // Шкала заполнена?
        bool isClaimed = currentTask.isCompleted;      // Кнопка забора уже нажималась?

        // Показываем галочку "Выполнено" только если награда РЕАЛЬНО ЗАБРАНА игроком
        if (completedBadge != null)
            completedBadge.SetActive(isClaimed);

        // Кнопка активна и видна, пока награда НЕ забрана
        if (claimButton != null)
            claimButton.gameObject.SetActive(!isClaimed);

        if (claimButtonText != null && claimButton != null)
        {
            if (isClaimed)
            {
                claimButtonText.text = "Получено";
                claimButton.interactable = false;
            }
            else if (isTargetReached)
            {
                // Ура! Квест выполнен, кнопка загорается сочным призывом к действию
                claimButtonText.text = $"Получить {currentTask.reward}";
                claimButton.interactable = true;
            }
            else
            {
                // Еще плестись и плестись до выполнения...
                claimButtonText.text = $"Награда: {currentTask.reward}";
                claimButton.interactable = false;
            }
        }
    }

    private void OnClaimClicked()
    {
        if (currentTask == null) return;
        if (currentTask.isCompleted) return; // Уже забирали награду
        if (!currentTask.IsComplete) return; // Еще не доползли до цели

        SFXManager.Instance?.PlayBueItem(); // Проигрываем твой любимый звук

        // ВЫРЕЗАЛИ ручное изменение флага и сохранение отсюда!
        // Теперь мы просто рапортуем менеджеру наверх: "Эй, игрок нажал 'Забрать'!".
        // Менеджер сам пометит статус, начислит валюту, сохранит и скажет обновить UI.
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