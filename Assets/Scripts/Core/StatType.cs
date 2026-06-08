using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Tech = 0,
    Human = 1,
    Manager = 2,
    Worker = 3,
    Introvert = 4,
    Extrovert = 5,
    Analyst = 6,
    Intuitive = 7,
    Stability = 8,
    Openness = 9
}

public static class StatTypeUtils
{
    public const int Count = 10;
    private static readonly string[] displayNames = new string[Count]
    {
        "Технический","Гуманитарный","Управленческий","Рабочий","Интроверт",
        "Экстраверт","Аналитик","Интуитивный","Стабильность","Открытость"
    };

    public static int ToIndex(this StatType t)
    {
        int i = (int)t;
        return (i >= 0 && i < Count) ? i : -1;
    }

    public static StatType FromIndex(int index)
    {
        if (index < 0 || index >= Count) return StatType.Tech;
        return (StatType)index;
    }

    public static string GetDisplayName(this StatType t)
    {
        int i = t.ToIndex();
        if (i >= 0 && i < displayNames.Length) return displayNames[i];
        return t.ToString();
    }

    public static IReadOnlyList<StatType> GetAll()
    {
        var arr = new StatType[Count];
        for (int i = 0; i < Count; i++) arr[i] = (StatType)i;
        return arr;
    }

    public static bool IsValidIndex(int index) => index >= 0 && index < Count;
}
