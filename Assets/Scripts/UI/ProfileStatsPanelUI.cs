using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileStatsPanelUI : MonoBehaviour
{
    public TMP_Text totalRunsText;
    public TMP_Text recommendedText;

    public Button goToMenu;
    public Button goToRun;
    public Button findMore;

    public Transform contentParent;
    public ProfessionStatsItemUI itemPrefab;

    private void Start()
    {
        goToMenu.onClick.AddListener(OnGoToMenu);
        goToRun.onClick.AddListener(OnGoToRun);
        findMore.onClick.AddListener(OnFindMore);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var analyzer = PreferenceAnalyzer.Instance;

        // ДОБАВЬТЕ ДЛЯ ОТЛАДКИ
        Debug.Log($"Total runs: {analyzer.GetTotalRuns()}");
        Debug.Log($"Min runs needed: {analyzer.minRunsForRecommendation}");
        Debug.Log($"Total successes: {analyzer.GetTotalSuccessChallenges()}");
        Debug.Log($"Min successes needed: {analyzer.minSuccessChallenges}");
        Debug.Log($"Can show recommendation: {analyzer.CanShowRecommendation()}");

        totalRunsText.text = $"Всего забегов: {analyzer.GetTotalRuns()}";

        var recommended = analyzer.GetDominantProfession();
        recommendedText.text = $"Рекомендуемая профессия: {recommended}";

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var stats = analyzer.GetAllStats();

        Debug.Log($"Creating {stats.Count} stat items");

        foreach (var stat in stats)
        {
            var item = Instantiate(itemPrefab, contentParent);
            item.Setup(stat);
            Debug.Log($"Created item for {stat.type}: weight={stat.weight}");
        }
    }

    // ----------------------------
    // BUTTON ACTIONS
    // ----------------------------

    private void OnGoToMenu()
    {
        GameManager.Instance.GoToMenu();
    }

    private void OnGoToRun()
    {
        GameManager.Instance.StartGame();
    }

    private void OnFindMore()
    {
        GameManager.Instance.GoToMiniGames();
    }
}