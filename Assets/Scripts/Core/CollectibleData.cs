using UnityEngine;

[CreateAssetMenu(menuName = "Game/CollectibleData", fileName = "NewCollectible")]
public class CollectibleData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea(2, 4)]
    public string description;

    [Header("Prefab / Visuals")]
    public GameObject prefab;
    public Sprite icon;

    [Header("Gameplay")]
    public bool isGem = false;
    public ProfessionData.ProfessionCategory category = ProfessionData.ProfessionCategory.None;
    [Min(0)] public float spawnWeight = 1f;

    [Header("Stat Effects")]
    public StatType PrimaryStatType = StatType.Tech;
    public int PrimaryValue = 0;
    public StatType SecondaryStatType = StatType.Tech;
    public int SecondaryValue = 0;

    [Header("UX")]
    [TextArea(2, 4)]
    public string thoughtText;
    public bool triggersSlowMo = false;
    public bool isRare = false;

    [Header("Editor / Debug")]
    [TextArea(2, 4)]
    public string devNotes;

    public bool IsProfessionCollectible()
    {
        return !isGem && category != ProfessionData.ProfessionCategory.None;
    }

    public string GetKey()
    {
        if (!string.IsNullOrEmpty(id)) return id;
        return name;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnWeight < 0f) spawnWeight = 0f;
        if (PrimaryValue < 0) PrimaryValue = 0;
        if (SecondaryValue < 0) SecondaryValue = 0;

        if (isGem)
        {
            category = ProfessionData.ProfessionCategory.None;
        }

        if (string.IsNullOrEmpty(id))
        {
            id = $"{name}_{(int)category}";
        }
    }
#endif
}
