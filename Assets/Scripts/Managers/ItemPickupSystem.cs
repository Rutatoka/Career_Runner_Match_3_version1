using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Простая система подбора предметов:
/// - хранит текущий доступный предмет (ItemData)
/// - хранит инвентарь (List<ItemData>)
/// - вызывает события при появлении предмета и при подборе
/// </summary>
public class ItemPickupSystem : MonoBehaviour
{
    public event Action<ItemData> OnItemAvailable;
    public event Action OnInventoryChanged;
    public event Action<ItemData> OnItemPicked;

    private ItemData currentItem;
    private readonly List<ItemData> inventory = new List<ItemData>();
    public int inventoryLimit = 50;

    public void SetCurrentItem(ItemData item)
    {
        currentItem = item;
        OnItemAvailable?.Invoke(currentItem);
    }

    public ItemData GetCurrentItem() => currentItem;

    public IReadOnlyList<ItemData> GetInventory() => inventory.AsReadOnly();

    public void PickupCurrentItem()
    {
        if (currentItem == null) return;
        if (!currentItem.isCollectible)
        {
            // maybe open preview
            return;
        }

        if (inventory.Count >= inventoryLimit)
        {
            Debug.LogWarning("Inventory full");
            return;
        }

        inventory.Add(currentItem);
        SaveOwnership(currentItem);
        OnInventoryChanged?.Invoke();
        OnItemPicked?.Invoke(currentItem);

        // Clear current item (picked)
        currentItem = null;
        OnItemAvailable?.Invoke(null);
    }

    private void SaveOwnership(ItemData item)
    {
        if (item == null) return;
        SaveSystem.MarkItemOwned(item.GetKey());
        if (item.coinValue > 0) SaveSystem.AddCoins(item.coinValue);
        if (item.gemValue > 0) SaveSystem.AddGems(item.gemValue);
    }

    public void ContinueAfterCategory()
    {
        // Hook for UI continue button; default does nothing
    }
}
