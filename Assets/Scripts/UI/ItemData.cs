using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemData", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public bool isCollectible = true;
    public int coinValue = 0;
    public int gemValue = 0;

    public string GetKey()
    {
        if (!string.IsNullOrEmpty(id)) return id;
        return displayName != null ? displayName.Replace(" ", "_").ToLowerInvariant() : System.Guid.NewGuid().ToString("N");
    }
}
