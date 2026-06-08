using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
  //  public Button button;

    private ItemData boundItem;
    private Action<ItemData> onClick;

    public void Setup(ItemData item, Action<ItemData> onClickCallback)
    {
        boundItem = item;
        onClick = onClickCallback;

        if (icon != null) icon.sprite = item?.icon;
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

        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.color = data.directionColor;
        }

        if (nameText != null)
            nameText.text = type.ToString();
    }






}
