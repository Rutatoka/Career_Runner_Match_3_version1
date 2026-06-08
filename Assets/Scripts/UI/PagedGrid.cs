using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PagedGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Transform gridContainer;
    public GameObject itemPrefab;
    public int itemsPerPage = 4;
    public int currentPage = 0;

    [Header("Navigation")]
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI pageText;

    private List<ShopItem> allItems = new List<ShopItem>();
    private List<GameObject> currentItems = new List<GameObject>();

    public void Setup(List<ShopItem> items)
    {
        allItems = items;
        currentPage = 0;
        RenderPage();
    }

    public void NextPage()
    {
        int maxPage = Mathf.CeilToInt((float)allItems.Count / itemsPerPage) - 1;
        if (currentPage < maxPage)
        {
            currentPage++;
            RenderPage();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RenderPage();
        }
    }

    private void RenderPage()
    {
        // Очищаем текущие предметы
        foreach (var item in currentItems)
        {
            Destroy(item);
        }
        currentItems.Clear();

        // Вычисляем начало и конец страницы
        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, allItems.Count);

        // Создаем предметы на странице
        for (int i = startIndex; i < endIndex; i++)
        {
            var obj = Instantiate(itemPrefab, gridContainer);
            obj.GetComponent<ShopItemUI>().Setup(allItems[i]);
            currentItems.Add(obj);
        }

        // Обновляем кнопки и текст
        if (pageText != null)
        {
            int totalPages = Mathf.CeilToInt((float)allItems.Count / itemsPerPage);
            pageText.text = $"{currentPage + 1}/{totalPages}";
        }

        if (prevButton != null)
            prevButton.interactable = currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = currentPage < Mathf.CeilToInt((float)allItems.Count / itemsPerPage) - 1;
    }
}