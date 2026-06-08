using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Transform container;
    public GameObject itemPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void AddProfession(ProfessionType type)
    {
        Debug.Log("AddProfession CALLED for: " + type);

        var data = ProfessionSystemDatabase.GetDataByType(type);
        if (data == null) return;

        GameObject item = Instantiate(itemPrefab, container);
        Debug.Log("Spawned UI item: " + item.name);

        var ui = item.GetComponent<InventoryItemUI>();
        if (ui != null)
            ui.SetupProfession(type);
    }

}
