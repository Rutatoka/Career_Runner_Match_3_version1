using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image iconBg;
    public Image iconItem;
    public TextMeshProUGUI nameText;
  //  public Button button;

    private ItemData boundItem;
    private Action<ItemData> onClick;

    public void Setup(ItemData item, Action<ItemData> onClickCallback)
    {
        boundItem = item;
        onClick = onClickCallback;

        if (iconBg != null) iconBg.sprite = item?.iconBg;
        if (iconItem != null) iconItem.sprite = item?.iconItem;
        if (nameText != null) nameText.text = item?.displayName ?? "Item";

        //if (button != null)
        //{
        //    button.onClick.RemoveAllListeners();
        //    button.onClick.AddListener(() => onClick?.Invoke(boundItem));
        //}
    }

    // НОВОЕ: настройка UI для профессий
    public void SetupProfession(ProfessionType type)
    {
       // Debug.Log("SetupProfession on: " + gameObject.name + " icon=" + icon.name);

        var data = ProfessionSystemDatabase.GetDataByType(type);

        if (iconItem != null)
        {
            iconItem.sprite = data.iconItem;
            iconBg.sprite = data.iconBg;
            iconItem.color = data.directionColor;
        }

        if (nameText != null)
            nameText.text = type.ToString();
    }






}
