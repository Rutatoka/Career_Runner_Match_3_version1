using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    public static List<ShopItem> GetCosmeticsList()
    {
        return new List<ShopItem>
        {
            //new ShopItem("Аватар программиста", "Стильный образ", 100, ShopItemType.Cosmetic),
            //new ShopItem("Дизайнерский костюм", "Для творческих", 150, ShopItemType.Cosmetic),
        };
    }

    public static List<ShopItem> GetClothList()
    {
        return new List<ShopItem>
        {
                  new ShopItem("Красная футболка", "Яркий красный цвет", 50, "torso", Color.red),
        new ShopItem("Синяя футболка", "Спокойный синий", 50, "torso", Color.blue),
        new ShopItem("Зеленая футболка", "Свежий зеленый", 50, "torso", Color.green),
        new ShopItem("Черные штаны", "Строгий черный", 40, "pants", Color.black),
        new ShopItem("Синие джинсы", "Классические джинсы", 60, "pants", new Color(0.2f, 0.4f, 0.7f)),
        new ShopItem("Коричневые ботинки", "Кожаные ботинки", 80, "shoes", new Color(0.55f, 0.27f, 0.07f)),
    
    };
    }

    public static List<ShopItem> GetAccessoryList()
    {
        return new List<ShopItem>
        {
            //new ShopItem("Очки", "Стильные очки", 10, ShopItemType.Accessory, "Accessories/Glasses"),
            //new ShopItem("Шляпа", "Ковбойская", 80, ShopItemType.Accessory, "Accessories/Hat"),
        };
    }

    public static List<ShopItem> GetAppearanceList()
    {
        return new List<ShopItem>
        {
            //new ShopItem("Прическа", "Модная стрижка", 10, ShopItemType.Appearance, "Appearances/Hair1"),
            //new ShopItem("Борода", "Стильная", 200, ShopItemType.Appearance, "Appearances/Beard"),
        };
    }

    public static List<ShopItem> GetBoostersList()
    {
        return new List<ShopItem>
        {
            //new ShopItem("Удвоить опыт", "30 минут", 50, ShopItemType.Booster),
            //new ShopItem("Ускорить тест", "Мгновенно", 75, ShopItemType.Booster),
        };
    }

    public static List<ShopItem> GetCustomizationList()
    {
        return new List<ShopItem>
        {
            //new ShopItem("Лидер или рабочий", "Реши кто ты", 200, ShopItemType.Tests),
            //new ShopItem("Уровень привязанности", "К месту, человеку", 300, ShopItemType.Tests),
        };
    }
}