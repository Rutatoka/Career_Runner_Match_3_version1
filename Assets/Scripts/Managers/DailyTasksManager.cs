using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyTasksManager : MonoBehaviour
{
    public static DailyTasksManager Instance { get; private set; }

    [Header("UI")]
    public Transform tasksContainer;
    public GameObject taskPrefab;
    public TextMeshProUGUI timerText;

    private List<DailyTask> tasks = new List<DailyTask>();
    private List<DailyTaskUI> taskUIs = new List<DailyTaskUI>();

    private const string LastDailyDateKey = "last_daily_date";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        tasks = GetDailyTasks();

        // 1. Сбрасываем квесты, если наступил новый день
        CheckDailyReset();

        // 2. Загружаем сохраненный прогресс
        LoadTasksProgress();

        // 3. Строим UI
        CreateOrRefreshTaskUI();

        // Запускаем тиканье таймера обновления
        InvokeRepeating(nameof(UpdateTimerDisplay), 0f, 1f);
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateTimerDisplay));
        if (Instance == this) Instance = null;
    }

    private List<DailyTask> GetDailyTasks()
    {
        return new List<DailyTask>
        {
            // Установили честные 15 предметов вместо 5!
            new DailyTask("pick_items", "Собери предметы", "Подбери 15 предметов", 0, 15, 50),
            new DailyTask("buy_item", "Купи предмет в магазине", "Купить 2 предмета одежды или аксессуара", 0, 2, 50),
            new DailyTask("earn_coins", "Заработай монет", "Собери 50 монет", 0, 50, 75)
        };
    }

    private void LoadTasksProgress()
    {
        foreach (var task in tasks)
        {
            string progressKey = $"daily_{task.id}_progress";
            string completedKey = $"daily_{task.id}_completed";

            int savedProgress = PlayerPrefs.GetInt(progressKey, 0);
            bool savedCompleted = PlayerPrefs.GetInt(completedKey, 0) == 1;

            task.Reset();
            task.AddProgress(savedProgress);

            if (savedCompleted)
            {
                task.MarkCompleted();
            }
        }
    }

    private void CreateOrRefreshTaskUI()
    {
        if (tasksContainer == null || taskPrefab == null)
        {
            Debug.LogError("DailyTasksManager: tasksContainer or taskPrefab not assigned.");
            return;
        }

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
            taskUIs.Clear();
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

        RefreshAllUI();
    }

    // Статический метод для вызова из других сцен (без авто-выдачи наград!)
    public static void AddProgress(string idOrTitle, int amount = 1)
    {
        string targetId = ResolveId(idOrTitle);
        if (string.IsNullOrEmpty(targetId)) return;

        // Если мы на сцене дейликов — обновляем UI в реальном времени
        if (Instance != null)
        {
            Instance.AddProgressToTask(targetId, amount);
            return;
        }

        // Если мы на другой сцене — просто пишем в сейвы без всяких авто-выдач
        string completedKey = $"daily_{targetId}_completed";
        if (PlayerPrefs.GetInt(completedKey, 0) == 1) return; // Награда уже была забрана ранее

        string progressKey = $"daily_{targetId}_progress";
        int current = PlayerPrefs.GetInt(progressKey, 0);
        int target = GetTargetForId(targetId);

        current = Mathf.Min(current + amount, target);
        PlayerPrefs.SetInt(progressKey, current);
        PlayerPrefs.Save();
    }

    public void AddProgressToTask(string idOrTitle, int amount = 1)
    {
        string targetId = ResolveId(idOrTitle);
        var task = tasks.Find(t => string.Equals(t.id, targetId, StringComparison.OrdinalIgnoreCase));

        if (task == null || task.isCompleted) return;

        task.AddProgress(amount);
        SaveTaskProgress(task);
        RefreshAllUI();

        // ВНИМАНИЕ: Из этого метода ПОЛНОСТЬЮ вырезана авто-выдача наград.
        // Игрок просто увидит заполненную шкалу и активную кнопку "Забрать".
    }

    // Этот метод срабатывает ТОЛЬКО тогда, когда игрок кликает по кнопке в UI!
    private void OnTaskClaimed(DailyTask task)
    {
        if (task == null) return;
        if (!task.IsComplete) return; // Ещё не накопил нужное количество
        if (task.isCompleted) return; // Уже нажал кнопку и забрал ранее

        // 1. Помечаем выполненным в памяти
        task.MarkCompleted();

        // 2. Сохраняем статус выполненности в сейв
        SaveTaskProgress(task);

        // 3. Выдаем гемы в SaveSystem
        ClaimTaskReward(task);

        // 4. Обновляем UI (кнопка исчезнет / сменится галочкой)
        RefreshAllUI();
    }

    private void ClaimTaskReward(DailyTask task)
    {
        SaveSystem.AddGems(task.reward);
        Debug.Log($"DailyTasksManager: Игрок вручную забрал {task.reward} гемов за '{task.title}'!");
        HeaderFooterManager.Instance?.Refresh();
    }

    private void SaveTaskProgress(DailyTask task)
    {
        if (task == null) return;
        PlayerPrefs.SetInt($"daily_{task.id}_progress", task.currentProgress);
        PlayerPrefs.SetInt($"daily_{task.id}_completed", task.isCompleted ? 1 : 0);
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

        var tempTasks = GetDailyTasks();
        foreach (var task in tempTasks)
        {
            PlayerPrefs.SetInt($"daily_{task.id}_progress", 0);
            PlayerPrefs.SetInt($"daily_{task.id}_completed", 0);
        }

        foreach (var task in tasks)
        {
            task.Reset();
        }

        PlayerPrefs.SetString(LastDailyDateKey, today);
        PlayerPrefs.Save();

        RefreshAllUI();
    }

    #region Вспомогательные статические методы

    private static string ResolveId(string idOrTitle)
    {
        if (string.IsNullOrEmpty(idOrTitle)) return null;
        string lower = idOrTitle.ToLower().Trim();

        if (lower == "pick_items" || lower == "собери предметы" || lower == "собери_предметы")
            return "pick_items";
        if (lower == "buy_item" || lower == "купи предмет в магазине" || lower == "купить 2 предмета одежды или аксессуара" || lower == "watch_ad")
            return "buy_item";
        if (lower == "earn_coins" || lower == "заработай монет" || lower == "заработай_монет")
            return "earn_coins";

        return idOrTitle;
    }

    private static int GetTargetForId(string id)
    {
        return id switch
        {
            "pick_items" => 15, // Здесь теперь тоже гордые 15!
            "buy_item" => 2,
            "earn_coins" => 50,
            _ => 100
        };
    }

    private static int GetRewardForId(string id)
    {
        return id switch
        {
            "pick_items" => 50,
            "buy_item" => 50,
            "earn_coins" => 75,
            _ => 0
        };
    }

    #endregion
}