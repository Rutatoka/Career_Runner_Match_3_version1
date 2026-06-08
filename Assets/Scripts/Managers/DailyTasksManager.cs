using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyTasksManager : MonoBehaviour
{
    [Header("UI")]
    public Transform tasksContainer;
    public GameObject taskPrefab;
    public TextMeshProUGUI timerText;

    private List<DailyTask> tasks = new List<DailyTask>();
    private List<DailyTaskUI> taskUIs = new List<DailyTaskUI>();

    private const string LastDailyDateKey = "last_daily_date";

    private void Start()
    {
        // Инициализация задач (можно заменить загрузкой из ScriptableObjects)
        tasks = GetDailyTasks();
        CreateOrRefreshTaskUI();

        // Синхронизируем прогресс с ItemPickupSystem (если есть)
        var pickup = FindObjectOfType<ItemPickupSystem>();
        if (pickup != null)
        {
            pickup.OnItemPicked += HandleItemPicked;
        }

        // Проверяем необходимость ресета по дате и запускаем таймер обновления UI
        CheckDailyReset();
        InvokeRepeating(nameof(UpdateTimerDisplay), 0f, 1f);
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateTimerDisplay));
        var pickup = FindObjectOfType<ItemPickupSystem>();
        if (pickup != null) pickup.OnItemPicked -= HandleItemPicked;
    }

    private List<DailyTask> GetDailyTasks()
    {
        // id, title, description, current, target, reward
        return new List<DailyTask>
        {
            new DailyTask("pick_items", "Собери предметы", "Подбери 5 предметов", 0, 5, 50),
            new DailyTask("watch_ad", "Посмотри рекламу", "Посмотри 1 рекламный ролик", 0, 1, 25),
            new DailyTask("earn_coins", "Заработай монет", "Собери 100 монет", 0, 100, 75),
        };
    }

    private void CreateOrRefreshTaskUI()
    {
        if (tasksContainer == null || taskPrefab == null)
        {
            Debug.LogError("DailyTasksManager: tasksContainer or taskPrefab not assigned.");
            return;
        }

        // Если UI ещё не созданы — создаём
        if (tasksContainer.childCount == 0)
        {
            taskUIs.Clear();
            foreach (var task in tasks)
            {
                var obj = Instantiate(taskPrefab, tasksContainer);
                var ui = obj.GetComponent<DailyTaskUI>();
                if (ui != null)
                {
                    ui.Setup(task, OnTaskClaimed);
                    taskUIs.Add(ui);
                }
            }
        }
        else
        {
            // Привязываем существующие UI к задачам
            int i = 0;
            foreach (Transform child in tasksContainer)
            {
                var ui = child.GetComponent<DailyTaskUI>();
                if (ui != null && i < tasks.Count)
                {
                    ui.Setup(tasks[i], OnTaskClaimed);
                    taskUIs.Add(ui);
                }
                i++;
            }
        }

        // Загружаем прогресс из PlayerPrefs (внутри UI)
        RefreshAllUI();
    }

    private void HandleItemPicked(ItemData item)
    {
        // Пример: любое подобранное collectible увеличивает прогресс задачи "pick_items"
        AddProgressToTask("pick_items", 1);
    }

    public void AddProgressToTask(string idOrTitle, int amount = 1)
    {
        if (string.IsNullOrEmpty(idOrTitle)) return;

        var task = tasks.Find(t => string.Equals(t.id, idOrTitle, StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(t.title, idOrTitle, StringComparison.OrdinalIgnoreCase));
        if (task == null) return;
        if (task.isCompleted) return;

        task.AddProgress(amount);
        SaveTaskProgress(task);
        RefreshAllUI();

        // Если задача завершилась — автоматически начисляем награду и помечаем
        if (task.IsComplete && !task.isCompleted)
        {
            task.MarkCompleted();
            SaveTaskProgress(task);
            ClaimTaskReward(task);
        }
    }

    private void OnTaskClaimed(DailyTask task)
    {
        if (task == null) return;
        if (!task.isCompleted) return;

        ClaimTaskReward(task);
        SaveTaskProgress(task);
        RefreshAllUI();
    }

    private void ClaimTaskReward(DailyTask task)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGems(task.reward);
            Debug.Log($"DailyTasksManager: awarded {task.reward} gems for {task.title}");
            HeaderFooterManager.Instance?.Refresh();
        }
        else
        {
            // Fallback: use SaveSystem
            SaveSystem.AddGems(task.reward);
            Debug.Log($"DailyTasksManager: (fallback) awarded {task.reward} gems for {task.title}");
        }
    }

    private void SaveTaskProgress(DailyTask task)
    {
        if (task == null) return;
        string progressKey = $"daily_{task.id}_progress";
        string completedKey = $"daily_{task.id}_completed";
        PlayerPrefs.SetInt(progressKey, task.currentProgress);
        PlayerPrefs.SetInt(completedKey, task.isCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void RefreshAllUI()
    {
        for (int i = 0; i < taskUIs.Count && i < tasks.Count; i++)
        {
            taskUIs[i].RefreshAll();
        }
    }

    private void UpdateTimerDisplay()
    {
        DateTime now = DateTime.Now;
        DateTime nextMidnight = now.Date.AddDays(1);
        TimeSpan remaining = nextMidnight - now;
        if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

        if (timerText != null)
            timerText.text = $"Обновление: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";

        // Если дата сменилась — ресетим
        string lastDate = PlayerPrefs.GetString(LastDailyDateKey, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (lastDate != today)
        {
            CheckDailyReset();
        }
    }

    private void CheckDailyReset()
    {
        string lastDate = PlayerPrefs.GetString(LastDailyDateKey, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (lastDate == today) return;

        foreach (var task in tasks)
        {
            task.Reset();
            PlayerPrefs.SetInt($"daily_{task.id}_progress", 0);
            PlayerPrefs.SetInt($"daily_{task.id}_completed", 0);
        }

        PlayerPrefs.SetString(LastDailyDateKey, today);
        PlayerPrefs.Save();

        RefreshAllUI();
    }
}
