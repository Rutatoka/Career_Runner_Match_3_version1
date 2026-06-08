using System;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    private const int STAT_COUNT = 10;
    private float[] runStats = new float[STAT_COUNT];
    private float[] exposurePrimary = new float[STAT_COUNT];
    private float[] exposureSecondary = new float[STAT_COUNT];

    public void AddStat(StatType type, int value, bool isPrimary)
    {
        int i = type.ToIndex();
        if (i < 0 || i >= STAT_COUNT) return;
        runStats[i] += value;
        if (isPrimary) exposurePrimary[i] += 1f; else exposureSecondary[i] += 1f;
    }

    public void ApplyObject(CollectibleData data)
    {
        if (data == null) return;
        if (data.PrimaryValue != 0) AddStat(data.PrimaryStatType, data.PrimaryValue, true);
        if (data.SecondaryValue != 0) AddStat(data.SecondaryStatType, data.SecondaryValue, false);
    }

    public void ResetRun()
    {
        Array.Clear(runStats, 0, STAT_COUNT);
        Array.Clear(exposurePrimary, 0, STAT_COUNT);
        Array.Clear(exposureSecondary, 0, STAT_COUNT);
    }

    public float[] GetVector()
    {
        var copy = new float[STAT_COUNT];
        Array.Copy(runStats, copy, STAT_COUNT);
        return copy;
    }

    public float[] GetNormalized()
    {
        float[] norm = new float[STAT_COUNT];
        for (int i = 0; i < STAT_COUNT; i++)
        {
            float exp = exposurePrimary[i] + exposureSecondary[i];
            norm[i] = exp > 0f ? runStats[i] / exp : 0f;
        }
        return norm;
    }

    public float GetConfidence()
    {
        float total = 0f;
        for (int i = 0; i < STAT_COUNT; i++) total += exposurePrimary[i] + exposureSecondary[i];
        return total;
    }

    public void CopyVectorTo(float[] target)
    {
        if (target == null || target.Length < STAT_COUNT) return;
        Array.Copy(runStats, target, STAT_COUNT);
    }

    public void CopyNormalizedTo(float[] target)
    {
        if (target == null || target.Length < STAT_COUNT) return;
        for (int i = 0; i < STAT_COUNT; i++)
        {
            float exp = exposurePrimary[i] + exposureSecondary[i];
            target[i] = exp > 0f ? runStats[i] / exp : 0f;
        }
    }
}
