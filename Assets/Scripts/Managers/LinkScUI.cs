using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LinkScUI : MonoBehaviour
{
    [Header("References")]
    public Transform contentParent;
    public LinkToCourseItemUI itemPrefab;

    [Header("Professions Ч назначь все 7 ScriptableObject'ов в инспекторе")]
    public List<ProfessionData> allProfessions;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("[LinkScUI] itemPrefab not assigned!");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("[LinkScUI] contentParent not assigned!");
            return;
        }

        // „истим старые карточки
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (PreferenceAnalyzer.Instance == null)
        {
            Debug.LogError("[LinkScUI] PreferenceAnalyzer.Instance is null!");
            return;
        }

        // ѕолучаем статистику и сортируем по весу Ч лучшее сверху
        List<ProfessionStatsView> stats = PreferenceAnalyzer.Instance
            .GetAllStats()
            .OrderByDescending(s => s.weight)
            .ToList();

        ProfessionType recommended =
            PreferenceAnalyzer.Instance.GetDominantProfession();

        foreach (var stat in stats)
        {
            // »щем ProfessionData дл€ этого типа профессии
            ProfessionData profData = allProfessions
                .FirstOrDefault(p => p.type == stat.type);

            if (profData == null)
            {
                Debug.LogWarning(
                    $"[LinkScUI] No ProfessionData found for {stat.type}");
                continue;
            }

            var item = Instantiate(itemPrefab, contentParent);
            bool isRecommended = stat.type == recommended;
            item.Setup(profData, stat, isRecommended);
        }
    }
}