using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AchievementUI : MonoBehaviour
{
    public TextMeshProUGUI achievementNameText;
    public TextMeshProUGUI achievementStatusText;
    public Image achievementIcon;
    public Sprite defaultIcon;
    public Color unlockedColor = new Color(0.15f, 0.7f, 0.15f);
    public Color inProgressColor = new Color(0.9f, 0.7f, 0.1f);

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(string achievementName, int progress, Sprite icon = null)
    {
        if (achievementNameText != null) achievementNameText.text = achievementName ?? "Достижение";
        if (achievementIcon != null) achievementIcon.sprite = icon != null ? icon : defaultIcon;
        if (achievementStatusText != null)
        {
            if (progress >= 1)
            {
                achievementStatusText.text = "Получено";
                achievementStatusText.color = unlockedColor;
            }
            else
            {
                achievementStatusText.text = "⏳ В процессе";
                achievementStatusText.color = inProgressColor;
            }
        }
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        canvasGroup.alpha = 0f;
        float t = 0f;
        float dur = 0.25f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / dur);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
