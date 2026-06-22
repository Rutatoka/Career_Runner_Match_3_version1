
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesignBackgroundController : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("Цвета")]
    public Color backgroundColor = new Color(0.07f, 0.04f, 0.12f);
    public Color gridColor = new Color(0.5f, 0.3f, 0.8f, 0.08f);
    public Color vignetteColor = new Color(0.04f, 0.02f, 0.08f, 0.5f);

    [Header("Сетка")]
    public int gridColumns = 6;
    public int gridRows = 10;

    [Header("Плавающие пятна")]
    public int blobCount = 10;
    public float blobMinSize = 60f;
    public float blobMaxSize = 160f;
    public float blobSpeed = 18f;

    private static readonly Color[] BlobColors = new Color[]
    {
        new Color(0.8f, 0.2f, 1.0f, 0.07f),   // фиолетовый
        new Color(1.0f, 0.3f, 0.6f, 0.06f),   // розовый
        new Color(0.3f, 0.6f, 1.0f, 0.06f),   // голубой
        new Color(1.0f, 0.7f, 0.2f, 0.05f),   // золотой
    //    new Color(0.2f, 0.9f, 0.7f, 0.05f),   // мятный
    };

    private RectTransform canvasRect;
    private readonly List<BlobData> blobs = new List<BlobData>();
    private Image vignetteImage;
    private float vignetteTime;

    private class BlobData
    {
        public RectTransform rect;
        public Image image;
        public Vector2 velocity;
        public float pulseOffset;
        public float pulseSpeed;
        public float baseSize;
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
     
        CreateGrid();
        CreateBlobs();
        CreateVignette();
        CreateBackground();
    }

    private void Update()
    {
        UpdateBlobs();
        PulseVignette();
    }

    // ??? ФОН ?????????????????????????????????????????????????????????
    private void CreateBackground()
    {
        var obj = CreateUIObject("BG_Background", targetCanvas.transform, -200);
        var img = obj.AddComponent<Image>();
        img.color = backgroundColor;
        StretchFull(obj.GetComponent<RectTransform>());
    }

    // ??? ДИАГОНАЛЬНАЯ СЕТКА ??????????????????????????????????????????
    private void CreateGrid()
    {
        var layer = CreateUIObject("BG_Grid", targetCanvas.transform, -190);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetCanvasWidth();
        float h = GetCanvasHeight();

        float cellW = w / gridColumns;
        float cellH = h / gridRows;

        // Вертикальные линии
        for (int c = 0; c <= gridColumns; c++)
        {
            var line = CreateUIObject($"VLine_{c}", layer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = gridColor;
            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1f, h);
            // Небольшой наклон — как дизайнерская сетка
            rect.localRotation = Quaternion.Euler(0f, 0f, 8f);
            rect.anchoredPosition = new Vector2(c * cellW - w * 0.5f, 0f);
        }

        // Горизонтальные линии
        for (int r = 0; r <= gridRows; r++)
        {
            var line = CreateUIObject($"HLine_{r}", layer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = gridColor;
            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(w, 1f);
            rect.anchoredPosition = new Vector2(0f, r * cellH - h * 0.5f);
        }
    }

    // ??? ПЯТНА КРАСКИ ????????????????????????????????????????????????
    private void CreateBlobs()
    {
        var layer = CreateUIObject("BG_Blobs", targetCanvas.transform, -185);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetCanvasWidth();
        float h = GetCanvasHeight();

        for (int i = 0; i < blobCount; i++)
        {
            var obj = CreateUIObject($"Blob_{i}", layer.transform, 0);
            var img = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();

            Color c = BlobColors[Random.Range(0, BlobColors.Length)];
            img.color = c;

            float size = Random.Range(blobMinSize, blobMaxSize);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size * Random.Range(0.6f, 1.4f));
            rect.anchoredPosition = new Vector2(
                Random.Range(-w * 0.5f, w * 0.5f),
                Random.Range(-h * 0.5f, h * 0.5f)
            );
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            // Случайная скорость и направление дрейфа
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(blobSpeed * 0.3f, blobSpeed);

            blobs.Add(new BlobData
            {
                rect = rect,
                image = img,
                velocity = new Vector2(
                    Mathf.Cos(angle) * speed,
                    Mathf.Sin(angle) * speed),
                pulseOffset = Random.Range(0f, Mathf.PI * 2f),
                pulseSpeed = Random.Range(0.3f, 0.8f),
                baseSize = size
            });
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
    private void UpdateBlobs()
    {
        float dt = Time.deltaTime;
        float w = GetCanvasWidth();
        float h = GetCanvasHeight();
        float t = Time.time;

        foreach (var b in blobs)
        {
            if (b.rect == null) continue;

            var pos = b.rect.anchoredPosition;
            pos += b.velocity * dt;

            // Отскок от краёв
            if (pos.x > w * 0.5f) { pos.x = w * 0.5f; b.velocity.x *= -1f; }
            if (pos.x < -w * 0.5f) { pos.x = -w * 0.5f; b.velocity.x *= -1f; }
            if (pos.y > h * 0.5f) { pos.y = h * 0.5f; b.velocity.y *= -1f; }
            if (pos.y < -h * 0.5f) { pos.y = -h * 0.5f; b.velocity.y *= -1f; }

            b.rect.anchoredPosition = pos;

            // Пульсация размера
            float pulse = (Mathf.Sin(t * b.pulseSpeed + b.pulseOffset) + 1f) * 0.5f;
            float size = Mathf.Lerp(b.baseSize * 0.8f, b.baseSize * 1.2f, pulse);
            b.rect.sizeDelta = new Vector2(size, size * 0.9f);

            // Медленное вращение
            b.rect.localRotation = Quaternion.Euler(
                0f, 0f,
                b.rect.localEulerAngles.z + dt * 8f
            );
        }
    }

    private void PulseVignette()
    {
        if (vignetteImage == null) return;
        vignetteTime += Time.deltaTime * 0.5f;
        Color c = vignetteColor;
        c.a = Mathf.Lerp(0.3f, 0.55f,
            (Mathf.Sin(vignetteTime) + 1f) * 0.5f);
        vignetteImage.color = c;
    }

    // ??? HELPERS ?????????????????????????????????????????????????????
    private float GetCanvasWidth()
    {
        float w = canvasRect.rect.width;
        return w > 10f ? w : Screen.width / targetCanvas.scaleFactor;
    }

    private float GetCanvasHeight()
    {
        float h = canvasRect.rect.height;
        return h > 10f ? h : Screen.height / targetCanvas.scaleFactor;
    }

    private GameObject CreateUIObject(string name, Transform parent, int siblingIndex)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        if (siblingIndex < 0)
            go.transform.SetAsFirstSibling();
        else
            go.transform.SetSiblingIndex(Mathf.Min(
                siblingIndex,
                parent.childCount));
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