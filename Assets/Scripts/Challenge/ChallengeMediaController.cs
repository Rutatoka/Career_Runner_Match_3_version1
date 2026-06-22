using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChallengeMediaController : MonoBehaviour
{
    // В начало класса, в секцию [Header("Settings")]
    [Header("Settings")]
    public float bpm = 120f;
    public float noteSpeed = 300f;
    public int totalNotes = 20;
    public float perfectThreshold = 30f;
    public float goodThreshold = 60f;
    [Range(0.25f, 4f)] public float noteSpacingMultiplier = 1.0f;   // 0.5 = восьмые, 1 = четверти, 2 = половинные
    [Range(0f, 1f)] public float requiredAccuracy = 0.7f;
    private bool ended = false;
    [Header("References")]
    public List<HitZone> hitZones;
    public ResultWindow resultWindow;
    public GameObject notePrefab;
    public Transform[] spawnPoints;
    public Transform[] targetPoints;
    public Canvas parentCanvas;                // Canvas, на котором всё происходит (если null – найдёт сам)

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text feedbackText;

    [Header("Effect Settings")]
    public float effectDuration = 0.6f;        // общее время жизни эффекта
    public float textFloatDistance = 50f;      // на сколько пикселей улетает текст вверх
    public float flashStartScale = 2f;         // начальный размер вспышки

    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private int notesHit = 0;
    private int notesMissed = 0;
    private bool finished = false;

    private List<(float time, int lane)> pattern;
    private float songTime = 0f;
    private int nextNoteIndex = 0;

    private void Start()
    {
        Debug.Log("[ChallengeMedia] Start");
        GeneratePattern();

        if (hitZones == null || hitZones.Count == 0)
            hitZones = new List<HitZone>(FindObjectsOfType<HitZone>());
        if (resultWindow == null)
            resultWindow = FindObjectOfType<ResultWindow>();
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();

        UpdateUI();
    }

    private void Update()
    {
        if (finished) return;

        songTime += Time.deltaTime;
        while (nextNoteIndex < pattern.Count && pattern[nextNoteIndex].time <= songTime)
        {
            SpawnNote(pattern[nextNoteIndex].lane);
            nextNoteIndex++;
        }

        if (Input.GetKeyDown(KeyCode.D)) TriggerLaneHit(0);
        if (Input.GetKeyDown(KeyCode.F)) TriggerLaneHit(1);
        if (Input.GetKeyDown(KeyCode.J)) TriggerLaneHit(2);
        if (Input.GetKeyDown(KeyCode.K)) TriggerLaneHit(3);

        if (nextNoteIndex >= pattern.Count && NoActiveNotes())
            EndGame();

        UpdateUI();
    }

    private void GeneratePattern()
    {
        pattern = new List<(float, int)>();
        float beatInterval = 60f / bpm;
        for (int i = 0; i < totalNotes; i++)
        {
            // Интервал между нотами = beatInterval * noteSpacingMultiplier
            float time = i * beatInterval * noteSpacingMultiplier;
            int lane = Random.Range(0, 4);
            pattern.Add((time, lane));
        }
        pattern.Sort((a, b) => a.time.CompareTo(b.time));
    }

    private void SpawnNote(int lane)
    {
        if (notePrefab == null || spawnPoints == null || lane >= spawnPoints.Length) return;
        GameObject noteObj = Instantiate(notePrefab, spawnPoints[lane].position, Quaternion.identity, transform);
        NoteController note = noteObj.GetComponent<NoteController>();
        if (note != null)
            note.Init(this, lane, targetPoints[lane], noteSpeed, perfectThreshold, goodThreshold);
    }

    // ------ Методы обратной связи и эффектов ------

    public void OnNoteHit(int lane, float accuracyDistance)
    {
        if (ended) return;

        string rating;
        int points;

        if (accuracyDistance <= perfectThreshold)
        {
            rating = "Perfect";
            points = 100;
        }
        else if (accuracyDistance <= goodThreshold)
        {
            rating = "Good";
            points = 50;
        }
        else
        {
            rating = "Bad";
            points = 10;
        }

        combo++;
        if (combo > maxCombo) maxCombo = combo;
        notesHit++;
        score += points * (1 + combo / 10);

        if (lane < hitZones.Count && hitZones[lane] != null)
            hitZones[lane].PlayHitFeedback(rating);

        ShowFeedback(
            rating,
            rating == "Perfect" ? Color.green
            : rating == "Good" ? Color.yellow
            : Color.red);

        // ДОБАВЬ: берём позицию HitZone в координатах Canvas
        Vector2 ringPos = GetHitZoneCanvasPosition(lane);
        MediaBackgroundController.Instance?.OnHit(lane, rating, ringPos);
    }

    // ДОБАВЬ новый приватный метод в ChallengeMediaController:
    private Vector2 GetHitZoneCanvasPosition(int lane)
    {
        if (lane >= hitZones.Count || hitZones[lane] == null)
            return Vector2.zero;

        RectTransform hitRect = hitZones[lane].GetComponent<RectTransform>();
        if (hitRect == null) return Vector2.zero;

        if (parentCanvas == null) return Vector2.zero;

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return Vector2.zero;

        // Переводим world position HitZone в локальные координаты Canvas
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            parentCanvas.worldCamera,
            hitRect.position);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            parentCanvas.worldCamera,
            out localPoint);

        return localPoint;
    }

    public void OnNoteMiss(int lane)
    {
        if (ended) return;
        combo = 0;
        notesMissed++;

        if (lane < hitZones.Count && hitZones[lane] != null)
            hitZones[lane].PlayMissFeedback();

        ShowFeedback("Miss", Color.gray);

        // Miss — только бары, без кольца
        MediaBackgroundController.Instance?.OnMiss(lane);
    }

    /// <summary>
    /// Создаёт UI-эффект попадания: вспышка-круг + всплывающий текст.
    /// </summary>
    private void SpawnHitEffect(string rating, Color color, Vector3 worldPosition)
    {
        if (parentCanvas == null) return;
        StartCoroutine(AnimateHitEffect(rating, color, worldPosition));
    }

    private void SpawnMissEffect(Vector3 worldPosition)
    {
        if (parentCanvas == null) return;
        StartCoroutine(AnimateMissEffect(worldPosition));
    }

    // Анимация попадания: вспышка + текст
    private IEnumerator AnimateHitEffect(string text, Color color, Vector3 worldPos)
    {
        // Преобразуем мировую позицию в локальную для canvas
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint);

        // --- Вспышка (круг) ---
        GameObject flashObj = new GameObject("HitFlash");
        flashObj.transform.SetParent(parentCanvas.transform, false);
        RectTransform flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchoredPosition = localPoint;
        flashRect.sizeDelta = Vector2.zero;

        Image flashImage = flashObj.AddComponent<Image>();
        // Используем встроенный спрайт круга (если его нет в ресурсах, можно нарисовать самим, см. ниже)
        flashImage.sprite = DefaultCircleSprite();
        flashImage.color = new Color(color.r, color.g, color.b, 0.7f);

        // Анимация: расширяем и затухаем
        float elapsed = 0f;
        float flashDuration = effectDuration * 0.5f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float scale = Mathf.Lerp(flashStartScale, 0.5f, t);
            flashRect.sizeDelta = new Vector2(100f * scale, 100f * scale);
            flashImage.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.7f, 0f, t));
            yield return null;
        }
        Destroy(flashObj);

        // --- Текст рейтинга ---
        GameObject textObj = new GameObject("HitText");
        textObj.transform.SetParent(parentCanvas.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = localPoint;
        textObj.AddComponent<CanvasRenderer>();
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        // Анимация: всплываем вверх и затухаем
        Vector2 startPos = localPoint;
        Vector2 endPos = startPos + Vector2.up * textFloatDistance;
        elapsed = 0f;
        float textDuration = effectDuration;
        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textDuration;
            textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            tmp.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        Destroy(textObj);
    }

    // Анимация промаха (только серый круг и текст "Miss")
    private IEnumerator AnimateMissEffect(Vector3 worldPos)
    {
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint);

        // Серый круг
        GameObject flashObj = new GameObject("MissFlash");
        flashObj.transform.SetParent(parentCanvas.transform, false);
        RectTransform flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchoredPosition = localPoint;
        flashRect.sizeDelta = Vector2.zero;
        Image flashImage = flashObj.AddComponent<Image>();
        flashImage.sprite = DefaultCircleSprite();
        flashImage.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        float elapsed = 0f;
        float flashDuration = effectDuration * 0.3f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float scale = Mathf.Lerp(1.5f, 0.3f, t);
            flashRect.sizeDelta = new Vector2(80f * scale, 80f * scale);
            flashImage.color = new Color(0.5f, 0.5f, 0.5f, Mathf.Lerp(0.7f, 0f, t));
            yield return null;
        }
        Destroy(flashObj);

        // Текст "Miss"
        GameObject textObj = new GameObject("MissText");
        textObj.transform.SetParent(parentCanvas.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = localPoint;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Miss";
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.gray;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        Vector2 startPos = localPoint;
        Vector2 endPos = startPos + Vector2.up * textFloatDistance * 0.7f;
        elapsed = 0f;
        float textDuration = effectDuration * 0.5f;
        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textDuration;
            textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            tmp.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        Destroy(textObj);
    }

    // Вспомогательная функция: возвращает спрайт круга.
    // Если нет своего спрайта, можно использовать примитив через Texture2D (см. альтернативу ниже).
    private Sprite DefaultCircleSprite()
    {
        // Простейший путь: если есть спрайт "Circle" в ресурсах, вернуть его.
        // Иначе создаём программно через Texture2D (годится для небольших кругов).
        if (_defaultCircleSprite == null)
        {
            // Создаём маленькую текстуру 32x32 с нарисованным кругом
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            int center = 16;
            int radius = 15;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    pixels[y * 32 + x] = dist <= radius ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _defaultCircleSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        return _defaultCircleSprite;
    }
    private Sprite _defaultCircleSprite;

    // Остальные вспомогательные методы без изменений...

    private bool NoActiveNotes() => FindObjectsOfType<NoteController>().Length == 0;

    private void TriggerLaneHit(int lane)
    {
        if (lane < hitZones.Count && hitZones[lane] != null)
            hitZones[lane].TriggerHit();
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            CancelInvoke(nameof(ClearFeedback));
            feedbackText.text = message;
            feedbackText.color = color;
            Invoke(nameof(ClearFeedback), 1.5f);
        }
    }
    private void ClearFeedback() { if (feedbackText != null) feedbackText.text = ""; }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (comboText != null) comboText.text = combo > 1 ? $"Combo: {combo}" : "";
    }

    private void EndGame()
    {
        EndChallenge();
    }
    private void EndChallenge()
    {
        if (ended) return;
        ended = true;
        finished = true;

        float accuracy = (totalNotes > 0) ? (float)notesHit / totalNotes : 0f;

        bool success = accuracy >= requiredAccuracy;

        if (success)
            ChallengeManager.Instance?.FinishChallenge(true);
        else
            ChallengeManager.Instance?.FailChallenge("low accuracy");

        if (resultWindow != null)
        {
            if (success)
                resultWindow.ShowSuccess(notesHit, totalNotes, songTime);
            else
                resultWindow.ShowFailure(notesHit, totalNotes, "low accuracy");
        }
    }
}