using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI professionText;
    public TextMeshProUGUI descriptionText;
    public RadarChartTexture radarChart;
    public TextMeshProUGUI[] skillBars;

    private void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null) { ShowError("Ошибка системы", "GameManager не найден"); return; }

        var stats = gm.PlayerStats;
        if (stats == null) { ShowError("Ошибка данных", "PlayerStats не найден"); return; }

        float[] normalized = stats.GetNormalized();
        if (radarChart != null && normalized != null) radarChart.UpdateChart(normalized);
        if (normalized != null) ShowSkills(normalized);

        var category = gm.CategoryResult;
        var direction = gm.DirectionResult;
        if (direction == null) { ShowError("Результат не определён", "Направление не найдено"); return; }

        var filtered = new List<ProfessionData>();
        if (gm.professions != null)
        {
            foreach (var p in gm.professions) if (p != null && p.category == category && !p.isCategory) filtered.Add(p);
        }

        if (filtered.Count == 0) { ShowError("Ошибка данных", "Нет направлений в категории"); return; }

        var matcher = new ProfessionMatcher(filtered);
        var (best, ranked) = matcher.GetBestMatch(stats.GetVector());

        string categoryName = GameManager.ProfessionUtils.GetCategoryName(category);
        professionText.text = $"{categoryName} → {direction.professionName}";
        float confidence = stats.GetConfidence();

        descriptionText.text =
            $"Категория: {categoryName}\n" +
            $"Направление: {direction.professionName}\n\n" +
            $"Топ-3 направлений:\n" +
            $"{GetSafeRank(ranked, 0)}\n" +
            $"{GetSafeRank(ranked, 1)}\n" +
            $"{GetSafeRank(ranked, 2)}\n\n" +
            $"Уверенность: {confidence:0}";
    }

    private void ShowError(string title, string message)
    {
        if (professionText != null) professionText.text = title;
        if (descriptionText != null) descriptionText.text = message;
    }

    private void ShowSkills(float[] normalized)
    {
        if (normalized == null) return;
        var all = StatTypeUtils.GetAll();
        int count = Mathf.Min(skillBars.Length, all.Count);
        for (int i = 0; i < count; i++)
        {
            string name = all[i].GetDisplayName();
            int percent = Mathf.RoundToInt(Mathf.Clamp01(i < normalized.Length ? normalized[i] : 0f) * 100f);
            skillBars[i].text = $"{name}: {percent}%";
        }
    }

    private string GetSafeRank(List<(ProfessionData profession, float score)> ranked, int index)
    {
        if (ranked == null || ranked.Count <= index) return "-";
        var item = ranked[index];
        if (item.profession == null || item.profession.isCategory) return "-";
        float percent = Mathf.Clamp01(item.score) * 100f;
        return $"{item.profession.professionName}\n<size=80%>{percent:0}% совпадения</size>";
    }
}
