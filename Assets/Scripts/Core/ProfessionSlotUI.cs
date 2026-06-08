using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для трёх слотов профессии:
/// - показывает иконки направления
/// - заполняет слоты по мере сбора
/// - сбрасывает при смене направления
/// - реагирует на события ProfessionSystem
/// </summary>
public class ProfessionSlotUI : MonoBehaviour
{
    public static ProfessionSlotUI Instance;

    [Header("UI Slots (3 images)")]
    public Image slot1;
    public Image slot2;
    public Image slot3;

    [Header("Default visuals")]
    public Sprite emptySprite;
    public Color emptyColor = new Color(1, 1, 1, 0.2f);

    [Header("Filled visuals")]
    public Sprite filledSprite; // можно оставить null, тогда используется цвет направления

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (ProfessionSystem.Instance != null)
        {
            ProfessionSystem.Instance.OnProgressChanged += UpdateSlots;
            ProfessionSystem.Instance.OnPortalReady += OnPortalReady;
        }

        ResetSlots();
    }

    private void OnDisable()
    {
        if (ProfessionSystem.Instance != null)
        {
            ProfessionSystem.Instance.OnProgressChanged -= UpdateSlots;
            ProfessionSystem.Instance.OnPortalReady -= OnPortalReady;
        }
    }

    // ---------------------------------------------------------
    // ОБНОВЛЕНИЕ СЛОТОВ
    // ---------------------------------------------------------
    private void UpdateSlots(ProfessionType type, int count)
    {
        if (type == ProfessionType.None || count == 0)
        {
            ResetSlots();
            return;
        }

        // Получаем цвет направления
        Color color = GetProfessionColor(type);

        // Заполняем слоты
        SetSlot(slot1, count >= 1, color);
        SetSlot(slot2, count >= 2, color);
        SetSlot(slot3, count >= 3, color);
    }

    private void SetSlot(Image img, bool filled, Color color)
    {
        if (img == null) return;

        if (filled)
        {
            img.color = color;
            if (filledSprite != null)
                img.sprite = filledSprite;
        }
        else
        {
            img.color = emptyColor;
            img.sprite = emptySprite;
        }
    }

    // ---------------------------------------------------------
    // ПОРТАЛ АКТИВИРОВАН — КРАСИВАЯ АНИМАЦИЯ (опционально)
    // ---------------------------------------------------------
    private void OnPortalReady(ProfessionType type)
    {
        // Можно добавить анимацию вспышки или тряски
        // Пока просто сбрасываем слоты
        ResetSlots();
    }

    // ---------------------------------------------------------
    // СБРОС
    // ---------------------------------------------------------
    public void ResetSlots()
    {
        SetSlot(slot1, false, Color.white);
        SetSlot(slot2, false, Color.white);
        SetSlot(slot3, false, Color.white);
    }

    // ---------------------------------------------------------
    // ВСПОМОГАТЕЛЬНОЕ — ЦВЕТ НАПРАВЛЕНИЯ
    // ---------------------------------------------------------
    private Color GetProfessionColor(ProfessionType type)
    {
        // Цвета из GDD
        return type switch
        {
            ProfessionType.IT => new Color32(0x00, 0x7A, 0xFF, 255),        // Синий
            ProfessionType.Design => new Color32(0xAF, 0x52, 0xDE, 255),    // Фиолетовый
            ProfessionType.Marketing => new Color32(0xFF, 0x95, 0x00, 255), // Оранжевый
            ProfessionType.Analytics => new Color32(0x34, 0xC7, 0x59, 255), // Зелёный
            ProfessionType.Media => new Color32(0xFF, 0x2D, 0x55, 255),     // Розовый
            ProfessionType.Engineering => new Color32(0xFF, 0xCC, 0x00, 255), // Жёлтый
            ProfessionType.Management => new Color32(0xFF, 0x3B, 0x30, 255), // Красный
            _ => Color.white
        };
    }
}
