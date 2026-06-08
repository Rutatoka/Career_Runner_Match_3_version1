using UnityEngine;

public static class ProfessionSystemDatabase
{
    private static ProfessionObjectData[] cache;

    public static void Init(ProfessionObjectData[] all)
    {
        cache = all;
    }

    public static ProfessionObjectData GetDataByType(ProfessionType type)
    {
        if (cache == null) return null;

        foreach (var d in cache)
        {
            if (d != null && d.professionType == type)
                return d;
        }

        return null;
    }
}
