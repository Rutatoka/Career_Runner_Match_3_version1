using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProfessionDatabase : MonoBehaviour
{
    public static ProfessionDatabase Instance;

    public List<ProfessionData> professions;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetIconItem(ProfessionType type)
    {
        foreach (var p in professions)
            if (p.type == type)
                return p.iconItem;

        return null;
    }
    public Sprite GetIconBg(ProfessionType type)
    {
        foreach (var p in professions)
            if (p.type == type)
                return p.iconBg;

        return null;
    }
}
