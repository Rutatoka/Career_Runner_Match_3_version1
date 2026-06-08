using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Отображает облачко реплики над персонажем:
/// - посимвольный вывод
/// - авто‑скрытие
/// - работает в slow‑mo (unscaled time)
/// </summary>
public class ThoughtBubble : MonoBehaviour
{
    public static ThoughtBubble Instance;

    [Header("References")]
    public Transform targetHead;          // точка над головой персонажа
    public CanvasGroup canvasGroup;       // для fade
    public TextMeshProUGUI textField;     // текст реплики
    public RectTransform bubbleRoot;      // сам пузырь

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2.5f, 0);
    public float typeSpeed = 0.03f;       // скорость печати
    public float visibleDuration = 2f;    // сколько держать текст

    private Coroutine currentRoutine;
    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cam = Camera.main;
        HideImmediate();
    }

    private void LateUpdate()
    {
        if (targetHead == null || bubbleRoot == null) return;

        // позиционирование над головой
        Vector3 worldPos = targetHead.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        bubbleRoot.position = screenPos;
    }

    // ---------------------------------------------------------
    // ПУБЛИЧНЫЙ МЕТОД
    // ---------------------------------------------------------
    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(text));
    }

    // ---------------------------------------------------------
    // ЛОГИКА ПОКАЗА
    // ---------------------------------------------------------
    private IEnumerator ShowRoutine(string text)
    {
        canvasGroup.alpha = 1f;
        textField.text = "";

        // посимвольный вывод
        foreach (char c in text)
        {
            textField.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        // держим текст
        yield return new WaitForSecondsRealtime(visibleDuration);

        // скрываем
        HideImmediate();
        currentRoutine = null;
    }

    // ---------------------------------------------------------
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ---------------------------------------------------------
    public void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (textField != null)
            textField.text = "";
    }
}
