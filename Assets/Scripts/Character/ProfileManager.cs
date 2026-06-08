using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField nameInput;
    public CharacterModelController modelController;

    [Header("Tabs")]
    public Button clothTab;
    public Button accesTab;
    public Button lookTab;
    public GameObject clothPanel;
    public GameObject accesPanel;
    public GameObject lookPanel;

    [Header("Grid")]
    public Transform grid;
    public GameObject itemPrefab;

    [Header("Buttons")]
    public Button saveButton;
    public Button genderMaleButton;
    public Button genderFemaleButton;

    private List<ShopItem> ownedItems = new List<ShopItem>();

    private void Start()
    {
        // Загружаем имя
        nameInput.text = GameManager.Instance.characterData.characterName;
        nameInput.onValueChanged.AddListener(OnNameChanged);

        // Настраиваем табы
        clothTab.onClick.AddListener(() => SwitchTab(0));
        accesTab.onClick.AddListener(() => SwitchTab(1));
        lookTab.onClick.AddListener(() => SwitchTab(2));

        // Кнопка сохранения
        saveButton.onClick.AddListener(SaveProfile);

        // Кнопки пола
        genderMaleButton.onClick.AddListener(() => SetGender(0));
        genderFemaleButton.onClick.AddListener(() => SetGender(1));

        // Убедитесь, что активна только первая панель
        clothPanel.SetActive(true);
        accesPanel.SetActive(false);
        lookPanel.SetActive(false);

        // Загружаем предметы
        LoadOwnedItems();
        LoadItemsForTab(0);
    }

    private void OnNameChanged(string newName)
    {
        GameManager.Instance.characterData.characterName = newName;
        if (modelController != null)
            modelController.UpdateCharacter(); // Для имени нужно пересоздать модель
    }

    private void SwitchTab(int index)
    {
        // Сначала скрываем все панели
        clothPanel.SetActive(false);
        accesPanel.SetActive(false);
        lookPanel.SetActive(false);

        // Затем показываем нужную
        switch (index)
        {
            case 0:
                clothPanel.SetActive(true);
                break;
            case 1:
                accesPanel.SetActive(true);
                break;
            case 2:
                lookPanel.SetActive(true);
                break;
        }

        LoadItemsForTab(index);
    }

    private void SetGender(int gender)
    {
        GameManager.Instance.characterData.gender = gender;
        if (modelController != null)
            modelController.UpdateCharacter(); // Для смены пола нужно пересоздать модель
    }

    private void LoadOwnedItems()
    {
        ownedItems.Clear();

        // Загружаем все предметы из базы
        List<ShopItem> allItems = new List<ShopItem>();
        allItems.AddRange(ItemDatabase.GetCosmeticsList());
        allItems.AddRange(ItemDatabase.GetClothList());
        allItems.AddRange(ItemDatabase.GetAccessoryList());
        allItems.AddRange(ItemDatabase.GetAppearanceList());
        allItems.AddRange(ItemDatabase.GetBoostersList());
        allItems.AddRange(ItemDatabase.GetCustomizationList());

        // Фильтруем только купленные
        foreach (var item in allItems)
        {
            string key = $"shop_{item.name}_{item.type}";
            if (SaveSystem.IsItemOwned(key))
            {
                ownedItems.Add(item);
            }
        }
        Debug.Log($"Загружено {ownedItems.Count} купленных предметов");
    }

    private void LoadItemsForTab(int tabIndex)
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        ShopItemType type = tabIndex == 0 ? ShopItemType.Cloth :
                          tabIndex == 1 ? ShopItemType.Accessory :
                          ShopItemType.Appearance;

        foreach (var item in ownedItems)
        {
            if (item.type == type)
            {
                var obj = Instantiate(itemPrefab, grid);
                var ui = obj.GetComponent<ProfileItemUI>();
                if (ui != null)
                {
                    ui.Setup(item, OnEquipItem);
                }
            }
        }
    }

    private void OnEquipItem(ShopItem item)
    {
        switch (item.type)
        {
            case ShopItemType.Cloth:
                // Меняем цвет нужной части тела
                switch (item.bodyPart)
                {
                    case "torso":
                        GameManager.Instance.characterData.torsoColor = item.itemColor;
                        break;
                    case "pants":
                        GameManager.Instance.characterData.pantsColor = item.itemColor;
                        break;
                    case "shoes":
                        GameManager.Instance.characterData.shoesColor = item.itemColor;
                        break;
                }
                // 👇 ДЛЯ ОДЕЖДЫ: обновляем только цвета (мгновенно)
                if (modelController != null)
                    modelController.UpdateClothColors();
                break;

            case ShopItemType.Accessory:
                GameManager.Instance.characterData.equippedAccessory = item.resourcePath;
                // 👇 ДЛЯ АКСЕССУАРОВ: нужно пересоздать модель (новые 3D объекты)
                if (modelController != null)
                    modelController.UpdateCharacter();
                break;

            case ShopItemType.Appearance:
                GameManager.Instance.characterData.equippedAppearance = item.resourcePath;
                // 👇 ДЛЯ ВНЕШНОСТИ: нужно пересоздать модель (новые 3D объекты)
                if (modelController != null)
                    modelController.UpdateCharacter();
                break;
        }

        Debug.Log($"Надето: {item.name}");
        GameManager.Instance.SaveCharacterData();

        LoadItemsForTab(GetCurrentTabIndex());
    }

    private int GetCurrentTabIndex()
    {
        if (clothPanel.activeSelf) return 0;
        if (accesPanel.activeSelf) return 1;
        return 2;
    }

    public void SaveProfile()
    {
        GameManager.Instance.SaveCharacterData();
        if (modelController != null)
            modelController.UpdateCharacter();
        Debug.Log("Профиль сохранён!");
    }
}