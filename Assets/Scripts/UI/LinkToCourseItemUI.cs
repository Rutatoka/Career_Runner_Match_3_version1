using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LinkToCourseItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button btnToCourse;
  //  public Image professionIcon;

    [Header("Visual")]
    public Image backgroundImage;
    public Color recommendedColor = new Color(0.9f, 0.7f, 0.1f, 1f);
    public Color normalColor = new Color(0.6f, 0.3f, 0.8f, 1f);

    private string currentURL;
    private static readonly Dictionary<ProfessionType, string> RussianNames =
        new Dictionary<ProfessionType, string>
    {
        { ProfessionType.IT,          "Айтишник"    },
        { ProfessionType.Design,      "Дизайнер"    },
        { ProfessionType.Marketing,   "Маркетолог"  },
        { ProfessionType.Analytics,   "Аналитик"    },
        { ProfessionType.Media,       "Медиа"       },
        { ProfessionType.Engineering, "Инженер"     },
        { ProfessionType.Management,  "Менеджер"    },
    };
    public void Setup(
        ProfessionData profData,
        ProfessionStatsView stats,
        bool isRecommended)
    {
        if (profData == null) return;

        if (titleText != null)
        {
            string russianName = RussianNames.TryGetValue(
                profData.type, out string name)
                ? name
                : profData.professionName;

            titleText.text = isRecommended
                ? $"{russianName} (Рекомендуется)"
                : russianName;
        }
        if (descriptionText != null)
        {
            descriptionText.text = !string.IsNullOrEmpty(profData.courseDescription)
         ? profData.courseDescription
         : $"Важность: {stats.weight:0} | Предметы: {stats.objects} | Порталы: {stats.portals}";
        }

        //   if (professionIcon != null && profData.icon != null)
        //        professionIcon.sprite = profData.icon;

        if (backgroundImage != null)
            backgroundImage.color = isRecommended
                ? recommendedColor
                : normalColor;

        currentURL = profData.courseURL;

        if (btnToCourse != null)
        {
            btnToCourse.onClick.RemoveAllListeners();
            btnToCourse.onClick.AddListener(OnCourseClicked);

            // Блокируем кнопку если ссылки нет
            btnToCourse.interactable = !string.IsNullOrEmpty(currentURL);
        }
    }

    private void OnCourseClicked()
    {
        if (string.IsNullOrEmpty(currentURL))
        {
            Debug.LogWarning($"[LinkToCourseItemUI] courseURL is empty!");
            return;
        }

        Application.OpenURL(currentURL);
    }
}