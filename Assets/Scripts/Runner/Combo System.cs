using UnityEngine;
using System;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    [Header("Settings")]
    public float comboDuration = 2.0f;     // время, за которое нужно сделать следующее действие
    public int maxCombo = 999;

    [Header("Multiplier Settings")]
    public float baseMultiplier = 1f;
    public float comboMultiplierStep = 0.05f; // +5% за каждое комбо

    [Header("UI")]
    public TMPro.TextMeshProUGUI comboText;
    public TMPro.TextMeshProUGUI multiplierText;

    private int comboCount = 0;
    private float timer = 0f;

    public event Action<int> OnComboChanged;
    public event Action OnComboBroken;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (comboCount > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                BreakCombo();
            }
        }
    }

    // ---------------------------------------------------------
    // ДОБАВИТЬ КОМБО
    // ---------------------------------------------------------
    public void AddCombo()
    {
        comboCount = Mathf.Min(comboCount + 1, maxCombo);
        timer = comboDuration;

        UpdateUI();
        OnComboChanged?.Invoke(comboCount);
    }

    // ---------------------------------------------------------
    // СБРОС КОМБО
    // ---------------------------------------------------------
    public void BreakCombo()
    {
        if (comboCount == 0)
            return;

        comboCount = 0;
        timer = 0f;

        UpdateUI();
        OnComboBroken?.Invoke();
    }

    // ---------------------------------------------------------
    // ПОЛУЧИТЬ ТЕКУЩИЙ МНОЖИТЕЛЬ
    // ---------------------------------------------------------
    public float GetMultiplier()
    {
        return baseMultiplier + comboCount * comboMultiplierStep;
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    private void UpdateUI()
    {
        if (comboText != null)
        {
            if (comboCount > 0)
                comboText.text = "COMBO x" + comboCount;
            else
                comboText.text = "";
        }

        if (multiplierText != null)
        {
            if (comboCount > 0)
                multiplierText.text = "×" + GetMultiplier().ToString("0.00");
            else
                multiplierText.text = "×1.00";
        }
    }

    // ---------------------------------------------------------
    // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ДРУГИХ СИСТЕМ
    // ---------------------------------------------------------
    public int GetCombo() => comboCount;
}
