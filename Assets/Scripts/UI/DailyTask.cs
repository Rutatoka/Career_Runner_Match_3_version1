using System;
using UnityEngine;

[Serializable]
public class DailyTask
{
    public string id;
    public string title;
    public string description;
    public int currentProgress;
    public int targetProgress;
    public int reward;
    public bool isCompleted; // Теперь это строго означает: "Награда ЗАБРАНА"

    public DailyTask(string id, string title, string description, int current, int target, int reward)
    {
        this.id = MakeSafeId(id ?? title);
        this.title = title ?? "";
        this.description = description ?? "";
        this.currentProgress = Mathf.Max(0, current);
        this.targetProgress = Mathf.Max(1, target);
        this.reward = Mathf.Max(0, reward);
        this.isCompleted = false;
    }

    // Квест готов к сдаче, если прогресс дополз до максимума
    public bool IsComplete => currentProgress >= targetProgress;

    public void AddProgress(int amount)
    {
        if (isCompleted) return; // Если награда уже забрана, прогресс больше не трогаем

        currentProgress = Mathf.Clamp(currentProgress + Mathf.Max(0, amount), 0, targetProgress);
        // УБРАЛИ автоматический запуск isCompleted = true отсюда!
    }

    public void Reset()
    {
        currentProgress = 0;
        isCompleted = false;
    }

    public void MarkCompleted()
    {
        isCompleted = true;
        currentProgress = targetProgress; // На всякий случай дотягиваем шкалу до топа
    }

    public static string MakeSafeId(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return Guid.NewGuid().ToString("N");
        var s = raw.Trim().ToLowerInvariant();
        s = s.Replace(" ", "_");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9_]", "");
        return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("N") : s;
    }
}