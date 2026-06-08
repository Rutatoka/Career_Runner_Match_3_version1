using UnityEngine;
using System.Collections.Generic;

public class ProfessionDatabase : MonoBehaviour
{
    public static ProfessionDatabase Instance;

    public List<ProfessionData> professions;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetIcon(ProfessionType type)
    {
        foreach (var p in professions)
            if (p.type == type)
                return p.icon;

        return null;
    }
}
