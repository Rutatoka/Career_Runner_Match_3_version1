using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManagementBackgroundController : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("Цвета фона")]
    public Color backgroundColor = new Color(0.12f, 0.08f, 0.05f);
    public Color boardColor = new Color(0.18f, 0.12f, 0.07f);
    public Color vignetteColor = new Color(0.05f, 0.03f, 0.02f, 0.7f);

    [Header("Стикеры")]
    public int stickerCount = 14;
    public float stickerSpeed = 12f;

    private static readonly Color[] StickerColors = new Color[]
    {
        new Color(0.95f, 0.25f, 0.25f, 0.55f),   // красный — срочно и важно
        new Color(0.95f, 0.85f, 0.15f, 0.50f),   // жёлтый — важно не срочно
        new Color(0.25f, 0.85f, 0.35f, 0.50f),   // зелёный — не важно
        new Color(0.95f, 0.25f, 0.25f, 0.30f),   // красный прозрачный
        new Color(0.95f, 0.85f, 0.15f, 0.28f),   // жёлтый прозрачный
    };

    private static readonly string[] StickerWords = new[]
    {
        "СРОЧНО!", "Дедлайн", "KPI", "Отчёт",
        "Встреча", "Задача", "Приоритет", "Plan B",
        "TODO", "ASAP", "Бриф", "Бюджет",
        "Клиент", "Спринт", "Митинг", "Риск",
        "Цель", "Команда", "Проект", "Статус"
    };

    private RectTransform canvasRect;
    private readonly List<StickerData> stickers = new List<StickerData>();
    private Image vignetteImage;
    private float vignetteTime;

    private class StickerData
    {
        public RectTransform rect;
        public Image image;
        public TextMeshProUGUI text;
        public float swayOffset;
        public float swaySpeed;
        public float swayAmount;
        public float driftX;
        public float driftY;
        public Vector2 basePosition;
        public float pulseOffset;
    }

    private void Start()
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas == null) return;

        canvasRect = targetCanvas.GetComponent<RectTransform>();

        StartCoroutine(BuildAfterLayout());
    }

    private IEnumerator BuildAfterLayout()
    {
        yield return null;
        yield return null;

        CreateBoardTexture();
        CreateStickers();
        CreatePins();
        CreateVignette();

        CreateBackground();
    }

    private void Update()
    {
        UpdateStickers();
        PulseVignette();
    }

    // ??? ФОН — ПРОБКОВАЯ ДОСКА ???????????????????????????????????????
    private void CreateBackground()
    {
        var obj = CreateUIObject("BG_Background", targetCanvas.transform, -200);
        obj.AddComponent<Image>().color = backgroundColor;
        StretchFull(obj.GetComponent<RectTransform>());
    }

    private void CreateBoardTexture()
    {
        // Имитируем фактуру доски горизонтальными полосками
        var layer = CreateUIObject("BG_Board", targetCanvas.transform, -195);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetW();
        float h = GetH();
        int lineCount = 24;
        float lineH = h / lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            var line = CreateUIObject($"BoardLine_{i}", layer.transform, 0);
            var img = line.AddComponent<Image>();

            // Чередующиеся тёмные полосы — имитация дерева
            float brightness = (i % 2 == 0) ? 0.0f : 0.03f;
            img.color = new Color(brightness, brightness * 0.6f,
                brightness * 0.3f, 0.4f);

            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(w, lineH - 1f);
            rect.anchoredPosition = new Vector2(0f, i * lineH - h * 0.5f);
        }
    }

    // ??? СТИКЕРЫ ?????????????????????????????????????????????????????
    private void CreateStickers()
    {
        var layer = CreateUIObject("BG_Stickers", targetCanvas.transform, -185);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetW();
        float h = GetH();

        for (int i = 0; i < stickerCount; i++)
        {
            var obj = CreateUIObject($"Sticker_{i}", layer.transform, 0);
            var img = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();

            // Размер стикера — прямоугольный
            float stickerW = Random.Range(90f, 150f);
            float stickerH = Random.Range(70f, 110f);

            Color col = StickerColors[Random.Range(0, StickerColors.Length)];
            img.color = col;

            Vector2 basePos = new Vector2(
                Random.Range(-w * 0.45f, w * 0.45f),
                Random.Range(-h * 0.45f, h * 0.45f)
            );

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(stickerW, stickerH);
            rect.anchoredPosition = basePos;
            rect.localRotation = Quaternion.Euler(
                0f, 0f, Random.Range(-18f, 18f));

            // Текст на стикере
            var textObj = CreateUIObject("StickerText", obj.transform, 0);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            var textRect = textObj.GetComponent<RectTransform>();

            text.text = StickerWords[Random.Range(0, StickerWords.Length)];
            text.fontSize = Random.Range(20f, 40f);
            text.color = new Color(0.1f, 0.05f, 0.02f, 0.85f);
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;

            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4f, 4f);
            textRect.offsetMax = new Vector2(-4f, -4f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            stickers.Add(new StickerData
            {
                rect = rect,
                image = img,
                text = text,
                swayOffset = Random.Range(0f, Mathf.PI * 2f),
                swaySpeed = Random.Range(0.3f, 0.8f),
                swayAmount = Random.Range(3f, 10f),
                driftX = Random.Range(-stickerSpeed, stickerSpeed) * 0.15f,
                driftY = Random.Range(-stickerSpeed, stickerSpeed) * 0.08f,
                basePosition = basePos,
                pulseOffset = Random.Range(0f, Mathf.PI * 2f)
            });
        }
    }

    // ??? КНОПКИ-БУЛАВКИ ??????????????????????????????????????????????
    private void CreatePins()
    {
        var layer = CreateUIObject("BG_Pins", targetCanvas.transform, -183);
        StretchFull(layer.GetComponent<RectTransform>());

        // Маленькие круглые точки в углах каждого стикера
        foreach (var s in stickers)
        {
            if (s.rect == null) continue;

            var pin = CreateUIObject("Pin", layer.transform, 0);
            var img = pin.AddComponent<Image>();
            var rect = pin.GetComponent<RectTransform>();

            img.color = new Color(0.6f, 0.1f, 0.1f, 0.8f);

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(10f, 10f);
            rect.anchoredPosition = s.basePosition +
                new Vector2(0f, s.rect.sizeDelta.y * 0.5f - 5f);
        }
    }

    // ??? ВИНЬЕТКА ????????????????????????????????????????????????????
    private void CreateVignette()
    {
        var obj = CreateUIObject("BG_Vignette", targetCanvas.transform, -180);
        StretchFull(obj.GetComponent<RectTransform>());
        vignetteImage = obj.AddComponent<Image>();
        vignetteImage.color = vignetteColor;
    }

    // ??? UPDATE ??????????????????????????????????????????????????????
    private void UpdateStickers()
    {
        float t = Time.time;
        float dt = Time.deltaTime;
        float w = GetW();
        float h = GetH();

        foreach (var s in stickers)
        {
            if (s.rect == null) continue;

            // Плавное покачивание — как приклеенный стикер
            float sway = Mathf.Sin(t * s.swaySpeed + s.swayOffset) * s.swayAmount;
            s.rect.localRotation = Quaternion.Euler(0f, 0f, sway);

            // Очень медленный дрейф
            var pos = s.rect.anchoredPosition;
            pos.x += s.driftX * dt;
            pos.y += s.driftY * dt;

            // Отскок от краёв
            float hw = s.rect.sizeDelta.x * 0.5f;
            float hh = s.rect.sizeDelta.y * 0.5f;

            if (pos.x > w * 0.5f - hw) { pos.x = w * 0.5f - hw; s.driftX *= -1f; }
            if (pos.x < -w * 0.5f + hw) { pos.x = -w * 0.5f + hw; s.driftX *= -1f; }
            if (pos.y > h * 0.5f - hh) { pos.y = h * 0.5f - hh; s.driftY *= -1f; }
            if (pos.y < -h * 0.5f + hh) { pos.y = -h * 0.5f + hh; s.driftY *= -1f; }

            s.rect.anchoredPosition = pos;

            // Пульсация прозрачности
            float pulse = (Mathf.Sin(t * 0.4f + s.pulseOffset) + 1f) * 0.5f;
            Color c = s.image.color;
            c.a = Mathf.Lerp(c.a * 0.85f, c.a * 1.1f, pulse);
            s.image.color = c;
        }
    }

    private void PulseVignette()
    {
        if (vignetteImage == null) return;
        vignetteTime += Time.deltaTime * 0.3f;
        Color c = vignetteColor;
        c.a = Mathf.Lerp(0.5f, 0.72f,
            (Mathf.Sin(vignetteTime) + 1f) * 0.5f);
        vignetteImage.color = c;
    }

    // ??? HELPERS ?????????????????????????????????????????????????????
    private float GetW() =>
        canvasRect.rect.width > 10f
            ? canvasRect.rect.width
            : Screen.width / targetCanvas.scaleFactor;

    private float GetH() =>
        canvasRect.rect.height > 10f
            ? canvasRect.rect.height
            : Screen.height / targetCanvas.scaleFactor;

    private GameObject CreateUIObject(string name, Transform parent, int siblingIndex)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;
        if (siblingIndex < 0)
            go.transform.SetAsFirstSibling();
        else
            go.transform.SetSiblingIndex(
                Mathf.Min(siblingIndex, parent.childCount));
        return go;
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}