using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HitZone : MonoBehaviour, IPointerDownHandler
{
    [Header("Settings")]
    public int laneIndex;
    public float hitRange = 80f;

    [Header("Visual Feedback")]
    public float hitScaleAmount = 0.85f;      // насколько сжимается при попадании
    public float hitAnimDuration = 0.08f;     // длительность сжатия
    public float missShakeAmount = 5f;        // амплитуда дрожания при промахе
    public float missAnimDuration = 0.15f;

    public Color perfectColor = Color.green;
    public Color goodColor = Color.yellow;
    public Color badColor = Color.red;
    public Color missColor = new Color(0.5f, 0.2f, 0.2f);

    private RectTransform rect;
    private Image zoneImage;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine currentAnim;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        zoneImage = GetComponent<Image>();
        originalColor = zoneImage != null ? zoneImage.color : Color.white;
        originalScale = rect.localScale;
    }

    public void TriggerHit()
    {
        Debug.Log($"[HitZone] TriggerHit lane {laneIndex}");

        NoteController[] notes = FindObjectsOfType<NoteController>();
        NoteController bestNote = null;
        float bestDistance = float.MaxValue;

        foreach (var note in notes)
        {
            // Принадлежит ли нота этой линии (сравниваем по X, допуск 10px)
            if (Mathf.Abs(note.transform.position.x - transform.position.x) < 10f)
            {
                float dist = note.GetDistanceToTarget();
                if (dist <= hitRange && dist < bestDistance)
                {
                    bestDistance = dist;
                    bestNote = note;
                }
            }
        }

        if (bestNote != null)
        {
            bestNote.TryHit();   // попадание обработается в контроллере ? вызовет PlayHitFeedback
        }
        else
        {
            Debug.Log($"[HitZone] No note in range – miss feedback only");
            // Пустое нажатие – только визуальная отдача, без штрафа
            PlayMissFeedback();
        }
    }

    /// <summary>
    /// Вызывается из ChallengeMediaController при успешном попадании.
    /// </summary>
    public void PlayHitFeedback(string rating)
    {
        Color color;
        switch (rating)
        {
            case "Perfect": color = perfectColor; break;
            case "Good": color = goodColor; break;
            default: color = badColor; break;
        }

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateHit(color));
    }

    /// <summary>
    /// Вызывается при промахе (нота ушла или пустое нажатие).
    /// </summary>
    public void PlayMissFeedback()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateMiss());
    }

    private IEnumerator AnimateHit(Color flashColor)
    {
        // Меняем цвет
        if (zoneImage != null) zoneImage.color = flashColor;

        // Сжатие
        float elapsed = 0f;
        while (elapsed < hitAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitAnimDuration;
            float scale = Mathf.Lerp(1f, hitScaleAmount, t);
            rect.localScale = originalScale * scale;
            yield return null;
        }

        // Возврат
        elapsed = 0f;
        while (elapsed < hitAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitAnimDuration;
            float scale = Mathf.Lerp(hitScaleAmount, 1f, t);
            rect.localScale = originalScale * scale;
            yield return null;
        }

        rect.localScale = originalScale;
        if (zoneImage != null) zoneImage.color = originalColor;
        currentAnim = null;
    }

    private IEnumerator AnimateMiss()
    {
        // Красный оттенок и лёгкое дрожание
        if (zoneImage != null) zoneImage.color = missColor;

        Vector3 startPos = rect.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < missAnimDuration)
        {
            elapsed += Time.deltaTime;
            float xShake = Mathf.Sin(elapsed * 50f) * missShakeAmount * (1f - elapsed / missAnimDuration);
            rect.anchoredPosition = startPos + new Vector3(xShake, 0f, 0f);
            yield return null;
        }

        rect.anchoredPosition = startPos;
        if (zoneImage != null) zoneImage.color = originalColor;
        currentAnim = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TriggerHit();
    }
}