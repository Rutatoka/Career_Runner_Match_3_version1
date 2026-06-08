using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI skillNameText;
    public Image skillFillImage;
    public TextMeshProUGUI skillValueText;

    public void Setup(string skillName, float value)
    {
        skillNameText.text = skillName;
        skillFillImage.fillAmount = value;
        skillValueText.text = $"{(value * 100):F0}%";
    }
}