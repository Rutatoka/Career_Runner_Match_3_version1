using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public Button equipButton;
    public GameObject equippedBadge;
    public TextMeshProUGUI buttonText;

    private ShopItem currentItem;
    private System.Action<ShopItem> onEquip;

    public void Setup(ShopItem item, System.Action<ShopItem> onEquipCallback)
    {
        currentItem = item;
        onEquip = onEquipCallback;

        nameText.text = item.name;
        UpdateUI();

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(OnEquipClicked);
    }

    private void UpdateUI()
    {
        bool isEquipped = IsEquipped(currentItem);
        equippedBadge.SetActive(isEquipped);
        equipButton.interactable = true;
        buttonText.text = isEquipped ? "Снять" : "Надеть";
    }

    private bool IsEquipped(ShopItem item)
    {
        if (GameManager.Instance == null) return false;

        switch (item.type)
        {
            case ShopItemType.Cloth:
                // Проверяем, какой цвет сейчас надет на эту часть тела
                switch (item.bodyPart)
                {
                    case "torso": return GameManager.Instance.characterData.torsoColor == item.itemColor;
                    case "pants": return GameManager.Instance.characterData.pantsColor == item.itemColor;
                    case "shoes": return GameManager.Instance.characterData.shoesColor == item.itemColor;
                    default: return false;
                }
            case ShopItemType.Accessory:
                return GameManager.Instance.characterData.equippedAccessory == item.resourcePath;
            case ShopItemType.Appearance:
                return GameManager.Instance.characterData.equippedAppearance == item.resourcePath;
            default:
                return false;
        }
    }

    private void OnEquipClicked()
    {
        if (onEquip != null)
        {
            if (IsEquipped(currentItem))
            {
                // Снимаем предмет
                switch (currentItem.type)
                {
                    case ShopItemType.Cloth:
                        // Снимаем одежду - возвращаем цвет по умолчанию (белый)
                        switch (currentItem.bodyPart)
                        {
                            case "torso":
                                GameManager.Instance.characterData.torsoColor = Color.white;
                                break;
                            case "pants":
                                GameManager.Instance.characterData.pantsColor = Color.white;
                                break;
                            case "shoes":
                                GameManager.Instance.characterData.shoesColor = Color.white;
                                break;
                        }
                        GameManager.Instance.SaveCharacterData();

                        // Обновляем цвета мгновенно
                        CharacterModelController model = FindAnyObjectByType<CharacterModelController>();
                        if (model != null)
                            model.UpdateClothColors();
                        break;

                    case ShopItemType.Accessory:
                        // Снимаем аксессуар
                        GameManager.Instance.characterData.equippedAccessory = "";
                        GameManager.Instance.SaveCharacterData();

                        // 👈 ВАЖНО: пересоздаем модель для удаления 3D объекта
                        CharacterModelController model2 = FindAnyObjectByType<CharacterModelController>();
                        if (model2 != null)
                            model2.UpdateCharacter();
                        break;

                    case ShopItemType.Appearance:
                        // Снимаем внешность
                        GameManager.Instance.characterData.equippedAppearance = "";
                        GameManager.Instance.SaveCharacterData();

                        // 👈 ВАЖНО: пересоздаем модель для удаления 3D объекта
                        CharacterModelController model3 = FindAnyObjectByType<CharacterModelController>();
                        if (model3 != null)
                            model3.UpdateCharacter();
                        break;
                }
            }
            else
            {
                // Надеваем предмет
                onEquip(currentItem);
            }
            UpdateUI();
        }
    }
}