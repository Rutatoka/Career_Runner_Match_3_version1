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
    public bool isCompleted;

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

    public bool IsComplete => currentProgress >= targetProgress || isCompleted;

    public void AddProgress(int amount)
    {
        if (IsComplete) return;
        currentProgress = Mathf.Clamp(currentProgress + Mathf.Max(0, amount), 0, targetProgress);
        if (currentProgress >= targetProgress) isCompleted = true;
    }

    public void Reset()
    {
        currentProgress = 0;
        isCompleted = false;
    }

    public void MarkCompleted()
    {
        isCompleted = true;
        currentProgress = targetProgress;
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
