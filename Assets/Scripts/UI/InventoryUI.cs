using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Transform container;
    public GameObject itemPrefab;

    private const int MaxItems = 12;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddProfession(ProfessionType type)
    {
        Debug.Log("AddProfession CALLED for: " + type);

        var data = ProfessionSystemDatabase.GetDataByType(type);
        if (data == null)
            return;
        Debug.Log("Current child count: " + container.childCount);
        // Если уже есть 12 предметов — удаляем самый старый
        if (container.childCount >= MaxItems)
        {
            Destroy(container.GetChild(0).gameObject);
        }

        GameObject item = Instantiate(itemPrefab, container);

        Debug.Log("Spawned UI item: " + item.name);

        var ui = item.GetComponent<InventoryItemUI>();
        if (ui != null)
        {
            ui.SetupProfession(type);
        }
    }
}