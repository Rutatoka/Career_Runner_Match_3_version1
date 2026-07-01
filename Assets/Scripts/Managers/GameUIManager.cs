using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Inventory UI")]
    public Transform inventoryContainer;
    public GameObject inventoryItemPrefab;

    [Header("Category Result UI")]
    public GameObject categoryResultPanel;
    public TextMeshProUGUI categoryText;
    public Button continueButton;
    public Button menuButton;
    public Button gameMenuButton;

    private ItemPickupSystem boundSystem;
    private readonly List<GameObject> inventoryItems = new List<GameObject>();

    private void Awake()
    {
        if (categoryResultPanel != null) categoryResultPanel.SetActive(false);
    }
    private void Start()
    {
        var ps = ProfessionSystem.Instance;
        if (ps != null)
            ps.OnProfessionCollected += HandleProfessionCollected;
    }
    private void HandleProfessionCollected(ProfessionType type)
    {
        AddProfessionToInventory(type);
    }
    private void AddProfessionToInventory(ProfessionType type)
    {
        if (inventoryContainer == null || inventoryItemPrefab == null)
            return;

        // 💀 УДАЛЯЕМ САМЫЙ СТАРЫЙ ЕСЛИ ПЕРЕБОР
        if (inventoryContainer.childCount >= 10)
        {
            Destroy(inventoryContainer.GetChild(0).gameObject);
        }

        var go = Instantiate(inventoryItemPrefab, inventoryContainer);
        var ui = go.GetComponent<InventoryItemUI>();

        if (ui != null)
            ui.SetupProfession(type);

        inventoryItems.Add(go);
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(ItemPickupSystem system)
    {
        Unbind();
        boundSystem = system ?? FindObjectOfType<ItemPickupSystem>();
        if (boundSystem == null) return;

        boundSystem.OnItemAvailable += HandleItemAvailable;
        boundSystem.OnInventoryChanged += HandleInventoryChanged;
        boundSystem.OnItemPicked += HandleItemPicked;

        if (menuButton != null) { menuButton.onClick.RemoveAllListeners(); menuButton.onClick.AddListener(GoToMenu); }
        if (gameMenuButton != null) { gameMenuButton.onClick.RemoveAllListeners(); gameMenuButton.onClick.AddListener(GoToMenu); }
        if (continueButton != null) { continueButton.onClick.RemoveAllListeners(); continueButton.onClick.AddListener(() => { categoryResultPanel?.SetActive(false); boundSystem.ContinueAfterCategory(); }); }

        RefreshInventoryUI();
        var current = boundSystem.GetCurrentItem();
        if (current != null) HandleItemAvailable(current);
        else ShowNoItem();
    }

    public void Unbind()
    {
        if (boundSystem != null)
        {
            boundSystem.OnItemAvailable -= HandleItemAvailable;
            boundSystem.OnInventoryChanged -= HandleInventoryChanged;
            boundSystem.OnItemPicked -= HandleItemPicked;
        }

        if (menuButton != null) menuButton.onClick.RemoveAllListeners();
        if (gameMenuButton != null) gameMenuButton.onClick.RemoveAllListeners();
        if (continueButton != null) continueButton.onClick.RemoveAllListeners();

        boundSystem = null;
    }

    private void HandleItemAvailable(ItemData item)
    {
        if (item == null) { ShowNoItem(); return; }
    }

    private void ShowNoItem()
    {
       // if (pickupPanel != null) pickupPanel.SetActive(false);
    }

    private void HandleInventoryChanged()
    {
        RefreshInventoryUI();
    }

    private void HandleItemPicked(ItemData item)
    {
        // При подборе — обновляем UI и даём прогресс задачам
        RefreshInventoryUI();
        var dtm = FindObjectOfType<DailyTasksManager>();
        dtm?.AddProgressToTask("pick_items", 1);
    }

    private void RefreshInventoryUI()
    {
        if (inventoryContainer == null || inventoryItemPrefab == null || boundSystem == null) return;

        foreach (var go in inventoryItems) if (go != null) Destroy(go);
        inventoryItems.Clear();

        var items = boundSystem.GetInventory();
        if (items == null) return;

        foreach (var item in items)
        {
            var go = Instantiate(inventoryItemPrefab, inventoryContainer);
            var ui = go.GetComponent<InventoryItemUI>();
            if (ui != null) ui.Setup(item, OnInventoryItemClicked);
            inventoryItems.Add(go);
        }
    }

    private void OnInventoryItemClicked(ItemData item)
    {
        HandleItemAvailable(item);
      //  if (pickupButton != null) pickupButton.interactable = false;
    }

    public void ShowCategoryResult()
    {
        if (categoryResultPanel == null || categoryText == null) return;
        categoryResultPanel.SetActive(true);
        var gm = GameManager.Instance;
        string categoryName = gm != null ? GameManager.ProfessionUtils.GetCategoryName(gm.CategoryResult) : "Неизвестно";
        categoryText.text = $"Твоя категория:\n<b>{categoryName}</b>";
    }

    public void GoToMenu()
    {
        if (GameManager.Instance != null) { GameManager.Instance.GoToMenu(); return; }
        SceneManager.LoadScene("MainMenu");
    }
}
