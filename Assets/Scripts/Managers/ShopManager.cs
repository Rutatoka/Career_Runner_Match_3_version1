//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class ShopManager : MonoBehaviour
//{
//    [Header("Tabs")]
//    public Button cosmeticsTab;
//    public Button boostersTab;
//    public Button customizationTab;

//    [Header("Panels")]
//    public GameObject cosmeticsPanel;
//    public GameObject boostersPanel;
//    public GameObject customizationPanel;

//    [Header("Scroll Rects")]
//    public ScrollRect cosmeticsScrollRect;
//    public ScrollRect boostersScrollRect;
//    public ScrollRect customizationScrollRect;

//    [Header("Content")]
//    public Transform cosmeticsContent;
//    public Transform boostersContent;
//    public Transform customizationContent;

//    [Header("Page Prefab")]
//    public GameObject pagePrefab;

//    [Header("Item Prefab")]
//    public GameObject itemPrefab;

//    private int itemsPerPage = 2;
//    private int itemsPerRow = 2;
//    private float pageWidth;
//    private bool isDragging = false;
//    private float dragStartPos;
//    private bool initialized;
//    private void Start()
//    {
//        if (initialized) return;
//        initialized = true;
//        cosmeticsTab.onClick.AddListener(() => SwitchTab(0));
//        boostersTab.onClick.AddListener(() => SwitchTab(1));
//        customizationTab.onClick.AddListener(() => SwitchTab(2));
//        Canvas.ForceUpdateCanvases();
//        // Вычисляем ширину страницы
//        pageWidth = cosmeticsScrollRect.viewport.rect.width;
//        Debug.Log($"Viewport width: {cosmeticsScrollRect.viewport.rect.width}");
//        Debug.Log($"Content width: {cosmeticsScrollRect.content.rect.width}");
//        LoadItems();
//        Invoke(nameof(ForceUpdate), 0.2f);
//    }
//    private void ForceUpdate()
//    {
//        Canvas.ForceUpdateCanvases();
//        Debug.Log($"Content width = {cosmeticsScrollRect.content.rect.width}");
//    }
//    private void SwitchTab(int index)
//    {
//        cosmeticsPanel.SetActive(index == 0);
//        boostersPanel.SetActive(index == 1);
//        customizationPanel.SetActive(index == 2);
//    }

//    private void LoadItems()
//    {
//        // Загружаем косметику
//        SetupScrollRect(cosmeticsScrollRect, cosmeticsContent, GetCosmeticsList());

//        // Загружаем бустеры
//        SetupScrollRect(boostersScrollRect, boostersContent, GetBoostersList());

//        // Загружаем кастомизацию (объединяем все предметы)
//        List<ShopItem> customizationItems = new List<ShopItem>();
//        customizationItems.AddRange(GetClothList());
//        customizationItems.AddRange(GetAccessoryList());
//        customizationItems.AddRange(GetAppearanceList());
//        SetupScrollRect(customizationScrollRect, customizationContent, customizationItems);
//    }

//    private void SetupScrollRect(ScrollRect scrollRect, Transform content, List<ShopItem> items)
//    {
//        scrollRect.StopMovement();
//        scrollRect.velocity = Vector2.zero;
//        scrollRect.enabled = false;
//        Debug.Log($"Начинаем загрузку {items.Count} предметов");
//        // Очищаем контент
//        foreach (Transform child in content)
//        {
//            Destroy(child.gameObject);
//        }

//        int totalItems = items.Count;
//        int totalPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);
//        Debug.Log($"Создаём {totalPages} страниц");
//        // Вычисляем ширину страницы (ширина Viewport)
//        float pageWidth = scrollRect.viewport.rect.width;

//        // Вычисляем высоту страницы (высота Viewport)
//        float pageHeight = scrollRect.viewport.rect.height;

//        for (int p = 0; p < totalPages; p++)
//        {
//            // Создаём страницу
//            GameObject page = Instantiate(pagePrefab, content);
//            RectTransform pageRect = page.GetComponent<RectTransform>();
//            //pageRect.sizeDelta = new Vector2(scrollRect.viewport.rect.width, scrollRect.viewport.rect.height);
//            // Настраиваем LayoutElement для страницы
//            var layout = page.GetComponent<LayoutElement>();
//            if (layout == null) layout = page.AddComponent<LayoutElement>();
//            layout.preferredWidth = pageWidth;
//            layout.preferredHeight = pageHeight;
//            Debug.Log($"Страница {p + 1} создана");
//            // Настраиваем GridLayoutGroup

//            // Заполняем предметами
//            int startIndex = p * itemsPerPage;
//            int endIndex = Mathf.Min(startIndex + itemsPerPage, totalItems);

//            for (int i = startIndex; i < endIndex; i++)
//            {
//                GameObject obj = Instantiate(itemPrefab, page.transform);
//                obj.GetComponent<ShopItemUI>().Setup(items[i]);
//            }
//            // Принудительно обновляем размеры
//            Debug.Log($"После обновления: Content width = {scrollRect.content.rect.width}");
//        }
//        //  Canvas.ForceUpdateCanvases();
//        Debug.Log(scrollRect.horizontalNormalizedPosition);
//        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.content.parent);
//        StartCoroutine(ResetScroll(scrollRect));        // Сбрасываем позицию скролла
//        StartCoroutine(FinishInit(scrollRect));
//    }
//    private IEnumerator FinishInit(ScrollRect sr)
//    {
//        yield return new WaitForEndOfFrame();

//        Canvas.ForceUpdateCanvases();
//        LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);

//        yield return new WaitForEndOfFrame();

//        sr.horizontalNormalizedPosition = 0f;

//        sr.velocity = Vector2.zero;
//        sr.StopMovement();

//        sr.enabled = true;
//    }
//    private System.Collections.IEnumerator ResetScroll(ScrollRect sr)
//    {
//        yield return null;
//        yield return new WaitForEndOfFrame();

//        Canvas.ForceUpdateCanvases();

//        sr.StopMovement();
//        sr.velocity = Vector2.zero;

//        sr.horizontalNormalizedPosition = 0f;
//    }
//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            cosmeticsScrollRect.horizontalNormalizedPosition += 0.1f;
//        }
//        // Обработка свайпа
//        if (Input.GetMouseButtonDown(0))
//        {
//            isDragging = true;
//            dragStartPos = Input.mousePosition.x;
//        }

//        if (Input.GetMouseButtonUp(0) && isDragging)
//        {
//            isDragging = false;
//            float dragDelta = Input.mousePosition.x - dragStartPos;

//            // Если свайп достаточно длинный, перелистываем страницу
//            if (Mathf.Abs(dragDelta) > 50) // 50 пикселей - порог
//            {
//                if (dragDelta < 0) // Свайп влево
//                {
//                    ScrollToNextPage(GetActiveScrollRect());
//                }
//                else // Свайп вправо
//                {
//                    ScrollToPrevPage(GetActiveScrollRect());
//                }
//            }
//            else
//            {
//                // Привязка к странице при коротком свайпе
//                SnapToPage(GetActiveScrollRect());
//            }
//        }
//    }

//    private ScrollRect GetActiveScrollRect()
//    {
//        if (cosmeticsPanel.activeSelf) return cosmeticsScrollRect;
//        if (boostersPanel.activeSelf) return boostersScrollRect;
//        return customizationScrollRect;
//    }

//    private void ScrollToNextPage(ScrollRect scrollRect)
//    {
//        float currentPos = scrollRect.horizontalNormalizedPosition;
//        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
//        int currentPage = Mathf.RoundToInt(currentPos * Mathf.Max(1, totalPages - 1));
//        int nextPage = Mathf.Clamp(currentPage + 1, 0, Mathf.Max(1, totalPages - 1));

//        float targetPos = (float)nextPage / Mathf.Max(1, totalPages - 1);
//        scrollRect.horizontalNormalizedPosition = targetPos;
//    }

//    private void ScrollToPrevPage(ScrollRect scrollRect)
//    {
//        float currentPos = scrollRect.horizontalNormalizedPosition;
//        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
//        int currentPage = Mathf.RoundToInt(currentPos * Mathf.Max(1, Mathf.Max(1, totalPages - 1)));
//        int prevPage = Mathf.Clamp(currentPage - 1, 0, Mathf.Max(1, Mathf.Max(1, totalPages - 1)));

//        float targetPos = (float)prevPage / Mathf.Max(1, Mathf.Max(1, totalPages - 1));
//        scrollRect.horizontalNormalizedPosition = targetPos;
//    }

//    private void SnapToPage(ScrollRect scrollRect)
//    {
//        float currentPos = scrollRect.horizontalNormalizedPosition;
//        int totalPages = Mathf.CeilToInt((float)GetTotalItemsForScrollRect(scrollRect) / itemsPerPage);
//        int pageIndex = Mathf.RoundToInt(currentPos * Mathf.Max(1, totalPages - 1));
//        pageIndex = Mathf.Clamp(pageIndex, 0, Mathf.Max(1, totalPages - 1));

//        float targetPos = (float)pageIndex / Mathf.Max(1, totalPages - 1);
//        scrollRect.horizontalNormalizedPosition = targetPos;
//    }

//    private int GetTotalItemsForScrollRect(ScrollRect scrollRect)
//    {
//        if (scrollRect == cosmeticsScrollRect) return GetCosmeticsList().Count;
//        if (scrollRect == boostersScrollRect) return GetBoostersList().Count;
//        if (scrollRect == customizationScrollRect)
//        {
//            int total = GetClothList().Count + GetAccessoryList().Count + GetAppearanceList().Count;
//            return total;
//        }
//        return 0;
//    }
//    private List<ShopItem> GetCosmeticsList() => ItemDatabase.GetCosmeticsList();
//    private List<ShopItem> GetClothList() => ItemDatabase.GetClothList();
//    private List<ShopItem> GetAccessoryList() => ItemDatabase.GetAccessoryList();
//    private List<ShopItem> GetAppearanceList() => ItemDatabase.GetAppearanceList();
//    private List<ShopItem> GetBoostersList() => ItemDatabase.GetBoostersList();
//    private List<ShopItem> GetCustomizationList() => ItemDatabase.GetCustomizationList();
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private int itemsPerPage = 2;
    private bool initialized;

    private void Start()
    {
        if (initialized) return;
        initialized = true;

        cosmeticsTab.onClick.AddListener(() => SwitchTab(0));
        boostersTab.onClick.AddListener(() => SwitchTab(1));
        customizationTab.onClick.AddListener(() => SwitchTab(2));

        StartCoroutine(InitShop());
    }

    private IEnumerator InitShop()
    {
        LoadItems();

        // даём Unity полностью пересчитать layout
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ResetScroll(cosmeticsScrollRect);
        ResetScroll(boostersScrollRect);
        ResetScroll(customizationScrollRect);
    }

    private void LoadItems()
    {
        SetupScrollRect(cosmeticsScrollRect, cosmeticsContent, GetCosmeticsList());
        SetupScrollRect(boostersScrollRect, boostersContent, GetBoostersList());

        List<ShopItem> customizationItems = new List<ShopItem>();
        customizationItems.AddRange(GetClothList());
        customizationItems.AddRange(GetAccessoryList());
        customizationItems.AddRange(GetAppearanceList());

        SetupScrollRect(customizationScrollRect, customizationContent, customizationItems);
    }

    private void SetupScrollRect(ScrollRect scrollRect, Transform content, List<ShopItem> items)
    {
        // очистка
        foreach (Transform child in content)
            Destroy(child.gameObject);

        if (items == null || items.Count == 0)
            return;

        int totalPages = Mathf.CeilToInt(items.Count / (float)itemsPerPage);

        for (int p = 0; p < totalPages; p++)
        {
            GameObject page = Instantiate(pagePrefab, content);

            int startIndex = p * itemsPerPage;
            int endIndex = Mathf.Min(startIndex + itemsPerPage, items.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                GameObject obj = Instantiate(itemPrefab, page.transform);
                obj.GetComponent<ShopItemUI>().Setup(items[i]);
            }
        }

        // важно: заставляем layout пересчитаться один раз
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.content);
    }

    private void ResetScroll(ScrollRect sr)
    {
        sr.StopMovement();
        sr.velocity = Vector2.zero;
        sr.horizontalNormalizedPosition = 0f;
    }

    private void SwitchTab(int index)
    {
        cosmeticsPanel.SetActive(index == 0);
        boostersPanel.SetActive(index == 1);
        customizationPanel.SetActive(index == 2);
    }

    // =========================
    // DATA
    // =========================

    private List<ShopItem> GetCosmeticsList() => ItemDatabase.GetCosmeticsList();
    private List<ShopItem> GetClothList() => ItemDatabase.GetClothList();
    private List<ShopItem> GetAccessoryList() => ItemDatabase.GetAccessoryList();
    private List<ShopItem> GetAppearanceList() => ItemDatabase.GetAppearanceList();
    private List<ShopItem> GetBoostersList() => ItemDatabase.GetBoostersList();
    private List<ShopItem> GetCustomizationList() => ItemDatabase.GetCustomizationList();
}