using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShopItem
{
    public string name;
    public string description;
    public int price;
    public ShopItemType type;
    public string resourcePath;
    public Color itemColor;
    public string bodyPart;

    public ShopItem(string name, string description, int price, ShopItemType type, string resourcePath = "")
    {
        this.name = name;
        this.description = description;
        this.price = price;
        this.type = type;
        this.resourcePath = resourcePath;
        this.itemColor = Color.white;
        this.bodyPart = "";
    }

    public ShopItem(string name, string description, int price, string bodyPart, Color color)
    {
        this.name = name;
        this.description = description;
        this.price = price;
        this.type = ShopItemType.Cloth;
        this.bodyPart = bodyPart;
        this.itemColor = color;
        this.resourcePath = "";
    }
}

public enum ShopItemType
{
    Cosmetic,
    Booster,
    Tests,
    Cloth,
    Accessory,
    Appearance
}

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public GameObject ownedBadge;
    public Image iconImage;

    private ShopItem currentItem;
    private bool isOwned = false;

    public void Setup(ShopItem item)
    {
        currentItem = item;
        isOwned = CheckIfOwned(item);
        UpdateUI();
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void UpdateUI()
    {
        nameText.text = currentItem.name;
        descriptionText.text = currentItem.description;

        if (isOwned)
        {
            priceText.text = "ОК";
            priceText.color = Color.green;
            buyButton.interactable = false;
            if (ownedBadge != null) ownedBadge.SetActive(true);
        }
        else
        {
            priceText.text = $"{currentItem.price}";
            priceText.color = Color.white;
            buyButton.interactable = true;
            if (ownedBadge != null) ownedBadge.SetActive(false);
        }
    }

    private bool CheckIfOwned(ShopItem item)
    {
        string key = $"shop_{item.name}_{item.type}";
        return SaveSystem.IsItemOwned(key);
    }

    private void OnBuyClicked()
    {
        if (isOwned) return;

        if (GameManager.Instance.GetGems() < currentItem.price)
        {
            Debug.Log($"Недостаточно гемов! Нужно: {currentItem.price}");
            return;
        }

        if (GameManager.Instance.SpendGems(currentItem.price))
        {
            MarkAsOwned();
            UpdateUI();

            if (HeaderFooterManager.Instance != null)
                HeaderFooterManager.Instance.Refresh();

            Debug.Log($"Куплено: {currentItem.name}");
        }
    }

    private void MarkAsOwned()
    {
        string key = $"shop_{currentItem.name}_{currentItem.type}";
        // SaveSystem.MarkItemOwned — правильное имя метода
        SaveSystem.MarkItemOwned(key);
        isOwned = true;
    }
}