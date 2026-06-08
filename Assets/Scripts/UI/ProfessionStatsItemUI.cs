using TMPro;
using UnityEngine;

public class ProfessionStatsItemUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text objects;
    public TMP_Text portals;
    public TMP_Text success;
    public TMP_Text fail;
    public TMP_Text successRate;
    public TMP_Text weight;

    public void Setup(ProfessionStatsView data)
    {
        title.text = data.type.ToString();

        objects.text = $"Предметы: {data.objects}";
        portals.text = $"Порталы: {data.portals}";
        success.text = $"Успехи: {data.successes}";
        fail.text = $"Провалы: {data.failures}";
        weight.text = $"Важность: {data.weight}";

        int total = data.successes + data.failures;

        float rate =
            total > 0
            ? (float)data.successes / total * 100f
            : 0f;

        successRate.text = $"Успешность: {rate:0}%";
    }
}