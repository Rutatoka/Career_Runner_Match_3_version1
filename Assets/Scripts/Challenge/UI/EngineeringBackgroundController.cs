using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EngineeringBackgroundController : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("Цвета")]
    public Color backgroundColor = new Color(0.02f, 0.05f, 0.12f);
    public Color gridColor = new Color(0.2f, 0.5f, 1.0f, 0.08f);
    public Color circuitColor = new Color(0.3f, 0.7f, 1.0f, 0.15f);
    public Color vignetteColor = new Color(0.01f, 0.03f, 0.08f, 0.5f);

    [Header("Сетка чертежа")]
    public int gridColumns = 10;
    public int gridRows = 16;

    [Header("Схема цепи")]
    public int circuitLineCount = 12;

    [Header("Мигающие узлы")]
    public int nodeCount = 8;

    private RectTransform canvasRect;
    private readonly List<NodeData> nodes = new List<NodeData>();
    private Image vignetteImage;
    private float vignetteTime;

    private class NodeData
    {
        public RectTransform rect;
        public Image image;
        public float pulseOffset;
        public float pulseSpeed;
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
        CreateCircuitLines();
        CreateNodes();
        CreateVignette();
        CreateBackground();
    }

    private void Update()
    {
        PulseNodes();
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

    // ??? СЕТКА ЧЕРТЕЖА ???????????????????????????????????????????????
    private void CreateGrid()
    {
        var layer = CreateUIObject("BG_Grid", targetCanvas.transform, -190);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetW();
        float h = GetH();

        float cellW = w / gridColumns;
        float cellH = h / gridRows;

        for (int c = 0; c <= gridColumns; c++)
        {
            var line = CreateUIObject($"V_{c}", layer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = gridColor;
            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1f, h);
            rect.anchoredPosition = new Vector2(c * cellW - w * 0.5f, 0f);
        }

        for (int r = 0; r <= gridRows; r++)
        {
            var line = CreateUIObject($"H_{r}", layer.transform, 0);
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

    // ??? ЛИНИИ СХЕМЫ ?????????????????????????????????????????????????
    private void CreateCircuitLines()
    {
        var layer = CreateUIObject("BG_Circuit", targetCanvas.transform, -185);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetW();
        float h = GetH();

        // Толстые горизонтальные линии схемы
        for (int i = 0; i < circuitLineCount; i++)
        {
            bool isHorizontal = Random.value > 0.4f;

            var line = CreateUIObject($"Circuit_{i}", layer.transform, 0);
            var img = line.AddComponent<Image>();
            img.color = circuitColor;
            var rect = line.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            if (isHorizontal)
            {
                float len = Random.Range(w * 0.1f, w * 0.5f);
                rect.sizeDelta = new Vector2(len, 2f);
                rect.anchoredPosition = new Vector2(
                    Random.Range(-w * 0.4f, w * 0.4f),
                    Random.Range(-h * 0.45f, h * 0.45f)
                );
            }
            else
            {
                float len = Random.Range(h * 0.05f, h * 0.25f);
                rect.sizeDelta = new Vector2(2f, len);
                rect.anchoredPosition = new Vector2(
                    Random.Range(-w * 0.45f, w * 0.45f),
                    Random.Range(-h * 0.4f, h * 0.4f)
                );
            }
        }
    }

    // ??? МИГАЮЩИЕ УЗЛЫ ???????????????????????????????????????????????
    private void CreateNodes()
    {
        var layer = CreateUIObject("BG_Nodes", targetCanvas.transform, -182);
        StretchFull(layer.GetComponent<RectTransform>());

        float w = GetW();
        float h = GetH();

        for (int i = 0; i < nodeCount; i++)
        {
            var obj = CreateUIObject($"Node_{i}", layer.transform, 0);
            var img = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();

            img.color = circuitColor;

            float size = Random.Range(15f, 25f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(
                Random.Range(-w * 0.35f, w * 0.35f),
                Random.Range(-h * 0.65f, h * 0.65f)
            );

            nodes.Add(new NodeData
            {
                rect = rect,
                image = img,
                pulseOffset = Random.Range(0f, Mathf.PI * 2f),
                pulseSpeed = Random.Range(1f, 3f)
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
    private void PulseNodes()
    {
        float t = Time.time;
        foreach (var n in nodes)
        {
            if (n.image == null) continue;
            float pulse = (Mathf.Sin(t * n.pulseSpeed + n.pulseOffset) + 1f) * 0.5f;
            Color c = n.image.color;
            c.a = Mathf.Lerp(0.15f, 0.8f, pulse);
            n.image.color = c;

            // Пульсация размера узла
            float size = Mathf.Lerp(20, 25f, pulse);
            n.rect.sizeDelta = new Vector2(size, size);
        }
    }

    private void PulseVignette()
    {
        if (vignetteImage == null) return;
        vignetteTime += Time.deltaTime * 0.4f;
        Color c = vignetteColor;
        c.a = Mathf.Lerp(0.3f, 0.5f,
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