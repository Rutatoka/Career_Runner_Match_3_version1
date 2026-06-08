using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Tabs")]
    public Button cosmeticsTab;
    public Button boostersTab;
    public Button customizationTab;

    [Header("Panels")]
    public GameObject cosmeticsPanel;
    public GameObject boostersPanel;
    public GameObject customizationPanel;

    [Header("Scroll Rects")]
    public ScrollRect cosmeticsScrollRect;
    public ScrollRect boostersScrollRect;
    public ScrollRect customizationScrollRect;

    [Header("Content")]
    public Transform cosmeticsContent;
    public Transform boostersContent;
    public Transform customizationContent;

    [Header("Page Prefab")]
    public GameObject pagePrefab;

    [Header("Item Prefab")]
    public GameObject itemPrefab;

    private int itemsPerPage = 4;
    private int itemsPerRow = 2;
    private float pageWidth;
    private bool isDragging = false;
    private float dragStartPos;

    private void Start()
    {
        cosmeticsTab.onClick.AddListener(() => SwitchTab(0));
        boostersTab.onClick.AddListener(() => SwitchTab(1));
        customizationTab.onClick.AddListener(() => SwitchTab(2));
        Canvas.ForceUpdateCanvases();
        // Вычисляем ширину страницы
        pageWidth = cosmeticsScrollRect.viewport.rect.width;
        Debug.Log($"Viewport width: {cosmeticsScrollRect.viewport.rect.width}");
        Debug.Log($"Content width: {cosmeticsScrollRect.content.rect.width}");
        LoadItems();
        Invoke(nameof(ForceUpdate), 0.2f);
    }
    private void ForceUpdate()
    {
        Canvas.ForceUpdateCanvases();
        Debug.Log($"Content width = {cosmeticsScrollRect.content.rect.width}");
    }
    private void SwitchTab(int index)
    {
        cosmeticsPanel.SetActive(index == 0);
        boostersPanel.SetActive(index == 1);
        customizationPanel.SetActive(index == 2);
    }

    private void LoadItems()
    {
        // Загружаем косметику
        SetupScrollRect(cosmeticsScrollRect, cosmeticsContent, GetCosmeticsList());

        // Загружаем бустеры
        SetupScrollRect(boostersScrollRect, boostersContent, GetBoostersList());

        // Загружаем кастомизацию (объединяем все предметы)
        List<ShopItem> customizationItems = new List<ShopItem>();
        customizationItems.AddRange(GetClothList());
        customizationItems.AddRange(GetAccessoryList());
        customizationItems.AddRange(GetAppearanceList());
        SetupScrollRect(customizationScrollRect, customizationContent, customizationItems);
    }

    private void SetupScrollRect(ScrollRect scrollRect, Transform content, List<ShopItem> items)
    {
        Debug.Log($"Начинаем загрузку {items.Count} предметов");
        // Очищаем контент
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        int totalItems = items.Count;
        int totalPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);
        Debug.Log($"Создаём {totalPages} страниц");
        // Вычисляем ширину страницы (ширина Viewport)
        float pageWidth = scrollRect.viewport.rect.width;

        // Вычисляем высоту страницы (высота Viewport)
        float pageHeight = scrollRect.viewport.rect.height;

        for (int p = 0; p < totalPages; p++)
        {
            // Создаём страницу
            GameObject page = Instantiate(pagePrefab, content);
            RectTransform pageRect = page.GetComponent<RectTransform>();
            pageRect.sizeDelta = new Vector2(scrollRect.viewport.rect.width, scrollRect.viewport.rect.height);
            // Настраиваем LayoutElement для страницы
            var layout = page.GetComponent<LayoutElement>();
            if (layout == null) layout = page.AddComponent<LayoutElement>();
            layout.preferredWidth = pageWidth;
            layout.preferredHeight = pageHeight;
            Debug.Log($"Страница {p + 1} создана");
            // Настраиваем GridLayoutGroup

            // Заполняем предметами
            int startIndex = p * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, totalItems);

            for (int i = startIndex; i < endIndex; i++)
            {
                GameObject obj = Instantiate(itemPrefab, page.transform);
                obj.GetComponent<ShopItemUI>().Setup(items[i]);
            }
            // Принудительно обновляем размеры
            Canvas.ForceUpdateCanvases();
            Debug.Log($"После обновления: Content width = {scrollRect.content.rect.width}");
        }

        // Сбрасываем позицию скролла
        scrollRect.horizontalNormalizedPosition = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cosmeticsScrollRect.horizontalNormalizedPosition += 0.1f;
        }
        // Обработка свайпа
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPos = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            float dragDelta = Input.mousePosition.x - dragStartPos;

            // Если свайп достаточно длинный, перелистываем страницу
            if (Mathf.Abs(dragDelta) > 50) // 50 пикселей - порог
            {
                if (dragDelta < 0) // Свайп влево
                {
                    ScrollToNextPage(GetActiveScrollRect());
                }
                else // Свайп вправо
                {
                    ScrollToPrevPage(GetActiveScrollRect());
                }
            }
            else
            {
                // Привязка к странице при коротком свайпе
                SnapToPage(GetActiveScrollRect());
            }
        }
    }

    private ScrollRect GetActiveScrollRect()
    {
        if (cosmeticsPanel.activeSelf) return cosmeticsScrollRect;
        if (boostersPanel.activeSelf) return boostersScrollRect;
        return customizationScrollRect;
    }

    private void ScrollToNextPage(ScrollRect scrollRect)
    {
        float currentPos = scrollRect.horizontalNormalizedPosition;
        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
        int currentPage = Mathf.RoundToInt(currentPos * (totalPages - 1));
        int nextPage = Mathf.Clamp(currentPage + 1, 0, totalPages - 1);

        float targetPos = (float)nextPage / (totalPages - 1);
        scrollRect.horizontalNormalizedPosition = targetPos;
    }

    private void ScrollToPrevPage(ScrollRect scrollRect)
    {
        float currentPos = scrollRect.horizontalNormalizedPosition;
        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
        int currentPage = Mathf.RoundToInt(currentPos * (totalPages - 1));
        int prevPage = Mathf.Clamp(currentPage - 1, 0, totalPages - 1);

        float targetPos = (float)prevPage / (totalPages - 1);
        scrollRect.horizontalNormalizedPosition = targetPos;
    }

    private void SnapToPage(ScrollRect scrollRect)
    {
        float currentPos = scrollRect.horizontalNormalizedPosition;
        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
        int pageIndex = Mathf.RoundToInt(currentPos * (totalPages - 1));
        pageIndex = Mathf.Clamp(pageIndex, 0, totalPages - 1);

        float targetPos = (float)pageIndex / (totalPages - 1);
        scrollRect.horizontalNormalizedPosition = targetPos;
    }

    private int GetTotalItemsForScrollRect(ScrollRect scrollRect)
    {
        if (scrollRect == cosmeticsScrollRect) return GetCosmeticsList().Count;
        if (scrollRect == boostersScrollRect) return GetBoostersList().Count;
        if (scrollRect == customizationScrollRect)
        {
            int total = GetClothList().Count + GetAccessoryList().Count + GetAppearanceList().Count;
            return total;
        }
        return 0;
    }
    private List<ShopItem> GetCosmeticsList() => ItemDatabase.GetCosmeticsList();
    private List<ShopItem> GetClothList() => ItemDatabase.GetClothList();
    private List<ShopItem> GetAccessoryList() => ItemDatabase.GetAccessoryList();
    private List<ShopItem> GetAppearanceList() => ItemDatabase.GetAppearanceList();
    private List<ShopItem> GetBoostersList() => ItemDatabase.GetBoostersList();
    private List<ShopItem> GetCustomizationList() => ItemDatabase.GetCustomizationList();
}