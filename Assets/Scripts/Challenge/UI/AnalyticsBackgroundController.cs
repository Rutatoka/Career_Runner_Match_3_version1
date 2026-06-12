using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalyticsBackgroundController : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("Цвета")]
    public Color backgroundColor = new Color(0.03f, 0.07f, 0.05f);
    public Color gridColor = new Color(0.0f, 0.5f, 0.3f, 0.15f);
    public Color symbolColor = new Color(0.0f, 0.9f, 0.5f, 0.6f);
    public Color glowColor = new Color(0.0f, 0.6f, 0.35f, 0.08f);

    [Header("Сетка")]
    public int gridColumns = 8;
    public int gridRows = 14;

    [Header("Летящие символы")]
    public int symbolCount =25;
    public float symbolSpeed = 80f;
    public float minFontSize = 45f;
    public float maxFontSize = 75f;

    private RectTransform canvasRect;
    private GameObject backgroundLayer;
    private GameObject gridLayer;
    private GameObject symbolLayer;
    private GameObject glowLayer;

    private readonly List<FlyingSymbol> flyingSymbols = new List<FlyingSymbol>();

    private static readonly string[] Symbols = new[]
    {
        "0","1","2","3","4","5","6","7","8","9",
        "?","?","?","?","?","?","?","%","#","@",
        "01","10","42","99","//","{}","[]","->","<>","=="
    };

    private class FlyingSymbol
    {
        public RectTransform rect;
        public TextMeshProUGUI text;
        public float speed;
        public float canvasHeight;
        public float alpha;
        public float alphaSpeed;
    }

    private void Start()
    {
        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogError("[AnalyticsBG] Canvas not found!");
            return;
        }

        canvasRect = targetCanvas.GetComponent<RectTransform>();

        // Порядок слоёв: фон ? сетка ? свечение ? символы

        StartCoroutine(BuildAfterLayout());
    
  
    }
    private System.Collections.IEnumerator BuildAfterLayout()
    {
        yield return null;
        yield return null;

        CreateGrid();
        CreateFlyingSymbols();
        // Свечение убираем — заменяем на тонкую виньетку
        CreateVignette();
        CreateBackground();
    }
    private Image vignetteImage;
    private void CreateVignette()
    {
        var obj = CreateUIObject("BG_Vignette", targetCanvas.transform, -180);
        StretchToCanvas(obj.GetComponent<RectTransform>());
        vignetteImage = obj.AddComponent<Image>();
        // Просто очень тёмные полупрозрачные края через цвет
        vignetteImage.color = new Color(0f, 0.15f, 0.08f, 0.35f);
    }
    private void Update()
    {
        UpdateFlyingSymbols();
        PulseGlow();
    }

    // ??? ФОНОВЫЙ ПРЯМОУГОЛЬНИК ???????????????????????????????????????
    private void CreateBackground()
    {
        backgroundLayer = CreateUIObject("BG_Background", targetCanvas.transform, -200);
        var img = backgroundLayer.AddComponent<Image>();
        img.color = backgroundColor;
        StretchToCanvas(backgroundLayer.GetComponent<RectTransform>());
    }

    // ??? СЕТКА ???????????????????????????????????????????????????????
    private void CreateGrid()
    {
        gridLayer = CreateUIObject("BG_Grid", targetCanvas.transform, -190);
        StretchToCanvas(gridLayer.GetComponent<RectTransform>());

        // Берём размеры через Screen если Canvas не отдал правильные
        float w = canvasRect.rect.width > 10f
            ? canvasRect.rect.width
            : Screen.width / targetCanvas.scaleFactor;
        float h = canvasRect.rect.height > 10f
            ? canvasRect.rect.height
            : Screen.height / targetCanvas.scaleFactor;

        float cellW = w / gridColumns;
        float cellH = h / gridRows;

        // Вертикальные линии
        for (int c = 0; c <= gridColumns; c++)
        {
            var line = CreateUIObject($"VLine_{c}", gridLayer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = gridColor;

            var rect = line.GetComponent<RectTransform>();
            // Anchor в центре — позиция считается от центра Canvas
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(3f, h);
            rect.anchoredPosition = new Vector2(
                c * cellW - w * 0.5f,
                0f
            );
        }

        // Горизонтальные линии
        for (int r = 0; r <= gridRows; r++)
        {
            var line = CreateUIObject($"HLine_{r}", gridLayer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = gridColor;

            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(w, 1f);
            rect.anchoredPosition = new Vector2(
                0f,
                r * cellH - h * 0.5f
            );
        }
    }

    // ??? СВЕЧЕНИЕ ????????????????????????????????????????????????????
    private GameObject glowObject;

    private void CreateGlowPulse()
    {
        glowLayer = CreateUIObject("BG_Glow", targetCanvas.transform, -185);
        glowObject = CreateUIObject("GlowCircle", glowLayer.transform, 0);
        StretchToCanvas(glowLayer.GetComponent<RectTransform>());

        var img = glowObject.AddComponent<Image>();
        img.color = glowColor;

        var rect = glowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600f, 600f);
    }

    private float glowTime;

    private void PulseGlow()
    {
        if (vignetteImage == null) return;
        glowTime += Time.deltaTime * 0.6f;
        float pulse = (Mathf.Sin(glowTime) + 1f) * 0.5f;
        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(0.25f, 0.45f, pulse);
        vignetteImage.color = c;
    }

    // ??? ЛЕТЯЩИЕ СИМВОЛЫ ?????????????????????????????????????????????
    private void CreateFlyingSymbols()
    {
        symbolLayer = CreateUIObject("BG_Symbols", targetCanvas.transform, -180);
        StretchToCanvas(symbolLayer.GetComponent<RectTransform>());

        float w = canvasRect.rect.width;
        float h = canvasRect.rect.height;

        for (int i = 0; i < symbolCount; i++)
        {
            var obj = CreateUIObject($"Symbol_{i}", symbolLayer.transform, 0);
            var text = obj.AddComponent<TextMeshProUGUI>();

            text.text = Symbols[Random.Range(0, Symbols.Length)];
            text.fontSize = Random.Range(minFontSize, maxFontSize);
            text.color = symbolColor;
            text.alignment = TextAlignmentOptions.Center;

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(80f, 40f);

            // Случайная стартовая позиция
            rect.anchoredPosition = new Vector2(
                Random.Range(-w * 0.5f, w * 0.5f),
                Random.Range(-h * 0.5f, h * 0.5f)
            );

            float speed = Random.Range(symbolSpeed * 0.4f, symbolSpeed * 1.4f);
            float alpha = Random.Range(0.2f, 0.7f);
            float alphaSpd = Random.Range(0.3f, 0.9f);

            Color sc = symbolColor;
            sc.a = alpha;
            text.color = sc;

            flyingSymbols.Add(new FlyingSymbol
            {
                rect = rect,
                text = text,
                speed = speed,
                canvasHeight = h,
                alpha = alpha,
                alphaSpeed = alphaSpd
            });
        }
    }

    private void UpdateFlyingSymbols()
    {
        float dt = Time.deltaTime;
        float h = canvasRect.rect.height;
        float w = canvasRect.rect.width;

        foreach (var s in flyingSymbols)
        {
            if (s.rect == null) continue;

            // Движение вниз
            var pos = s.rect.anchoredPosition;
            pos.y -= s.speed * dt;

            // Зацикливаем — уходит вниз, появляется сверху
            if (pos.y < -h * 0.5f - 40f)
            {
                pos.y = h * 0.5f + 40f;
                pos.x = Random.Range(-w * 0.5f, w * 0.5f);
                s.text.text = Symbols[Random.Range(0, Symbols.Length)];
            }

            s.rect.anchoredPosition = pos;

            // Пульсация прозрачности
            s.alpha += s.alphaSpeed * dt;
            float a = Mathf.Abs(Mathf.Sin(s.alpha)) * 0.6f + 0.1f;
            Color c = s.text.color;
            c.a = a;
            s.text.color = c;
        }
    }

    // ??? HELPERS ?????????????????????????????????????????????????????
    private GameObject CreateUIObject(string name, Transform parent, int siblingIndex)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        if (siblingIndex < 0)
            go.transform.SetAsFirstSibling();
        else
            go.transform.SetSiblingIndex(siblingIndex);

        return go;
    }

    private void StretchToCanvas(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}