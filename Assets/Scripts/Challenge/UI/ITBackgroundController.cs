using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ITBackgroundController : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("Цвета фона")]
    public Color backgroundColor = new Color(0.0f, 0.02f, 0.0f, 1f);
    public Color vignetteColor = new Color(0.0f, 0.0f, 0.0f, 0.75f);

    [Header("Слои матрицы")]
    public int farColumnCount = 14;
    public int midColumnCount = 10;
    public int nearColumnCount = 6;

    [Header("Скорости слоёв")]
    public float farSpeed = 180f;
    public float midSpeed = 110f;
    public float nearSpeed = 60f;

    [Header("Размеры шрифта")]
    public float farFontSize = 10f;
    public float midFontSize = 16f;
    public float nearFontSize = 26f;

    // Катакана + ASCII — настоящая матрица
    private static readonly string[] MatrixChars = new[]
    {
        "?","?","?","?","?","?","?","?","?","?",
        "?","?","?","?","?","?","?","?","?","?",
        "?","?","?","?","?","?","?","?","?","?",
        "0","1","2","3","4","5","6","7","8","9",
        "Z","X","C","V","B","N","M","T","R","E",
        "|",":","=","+","-","*","/","<",">"
    };

    private RectTransform canvasRect;
    private readonly List<MatrixColumn> columns = new List<MatrixColumn>();
    private Image vignetteImage;
    private float vignetteTime;

    private enum LayerType { Far, Mid, Near }

    private class MatrixColumn
    {
        public List<RectTransform> cells = new List<RectTransform>();
        public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        public float speed;
        public float x;
        public float headY;        // позиция "головы" потока
        public float canvasHeight;
        public float charHeight;
        public int length;       // длина хвоста
        public float changeTimer;
        public float changeInterval;
        public LayerType layer;
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

        CreateMatrixLayer(farColumnCount, farSpeed, farFontSize, LayerType.Far, -190);
        CreateMatrixLayer(midColumnCount, midSpeed, midFontSize, LayerType.Mid, -185);
        CreateMatrixLayer(nearColumnCount, nearSpeed, nearFontSize, LayerType.Near, -182);
        CreateVignette();
        CreateBackground();
    }

    private void Update()
    {
        UpdateMatrix();
        PulseVignette();
    }

    // ??? ФОН ?????????????????????????????????????????????????????????
    private void CreateBackground()
    {
        var obj = CreateUIObject("BG_Background", targetCanvas.transform, -200);
        obj.AddComponent<Image>().color = backgroundColor;
        StretchFull(obj.GetComponent<RectTransform>());
    }

    // ??? СЛОЙ МАТРИЦЫ ????????????????????????????????????????????????
    private void CreateMatrixLayer(
        int colCount, float speed, float fontSize,
        LayerType layer, int siblingIndex)
    {
        float w = GetW();
        float h = GetH();

        var layerObj = CreateUIObject($"BG_Matrix_{layer}",
            targetCanvas.transform, siblingIndex);
        StretchFull(layerObj.GetComponent<RectTransform>());

        float colWidth = w / colCount;
        float charH = fontSize * 1.3f;
        int tailLen = layer == LayerType.Near ? 8
                        : layer == LayerType.Mid ? 14
                        : 20;

        for (int c = 0; c < colCount; c++)
        {
            float xPos = c * colWidth - w * 0.5f + colWidth * 0.5f;

            var col = new MatrixColumn
            {
                speed = speed * Random.Range(0.7f, 1.3f),
                x = xPos,
                headY = Random.Range(-h * 0.5f, h * 0.5f),
                canvasHeight = h,
                charHeight = charH,
                length = tailLen + Random.Range(-3, 4),
                changeTimer = 0f,
                changeInterval = Random.Range(0.05f, 0.2f),
                layer = layer
            };

            for (int s = 0; s < col.length; s++)
            {
                var cell = CreateUIObject(
                    $"Cell_{layer}_{c}_{s}",
                    layerObj.transform, 0);

                var text = cell.AddComponent<TextMeshProUGUI>();
                text.text = MatrixChars[Random.Range(0, MatrixChars.Length)];
                text.fontSize = fontSize;
                text.alignment = TextAlignmentOptions.Center;

                // Голова — яркая белая, хвост затухает
                text.color = GetCellColor(s, col.length, layer);

                var rect = cell.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(colWidth, charH);
                rect.anchoredPosition = new Vector2(
                    xPos,
                    col.headY - s * charH
                );

                col.cells.Add(rect);
                col.texts.Add(text);
            }

            columns.Add(col);
        }
    }

    private Color GetCellColor(int index, int total, LayerType layer)
    {
        // index 0 = голова потока
        float t = (float)index / total;

        if (index == 0)
        {
            // Голова — ярко белая с зелёным оттенком
            float bright = layer == LayerType.Near ? 1f
                         : layer == LayerType.Mid ? 0.85f
                         : 0.6f;
            return new Color(bright * 0.8f, bright, bright * 0.8f, bright);
        }

        // Хвост затухает от зелёного к чёрному
        float alpha = Mathf.Lerp(0.8f, 0f, t);
        float green = Mathf.Lerp(0.9f, 0.1f, t);
        float layerMul = layer == LayerType.Far ? 0.45f
                       : layer == LayerType.Mid ? 0.7f
                       : 1.0f;

        return new Color(0f, green * layerMul, green * 0.2f * layerMul,
            alpha * layerMul);
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
    private void UpdateMatrix()
    {
        float dt = Time.deltaTime;

        foreach (var col in columns)
        {
            // Двигаем голову вниз
            col.headY -= col.speed * dt;

            // Зацикливаем когда хвост ушёл за край
            float tailY = col.headY - col.length * col.charHeight;
            if (col.headY < -col.canvasHeight * 0.5f - col.charHeight)
            {
                col.headY = col.canvasHeight * 0.5f +
                    Random.Range(0f, col.canvasHeight * 0.5f);
            }

            // Позиционируем все ячейки
            for (int s = 0; s < col.cells.Count; s++)
            {
                if (col.cells[s] == null) continue;
                col.cells[s].anchoredPosition = new Vector2(
                    col.x,
                    col.headY - s * col.charHeight
                );
            }

            // Периодически меняем символы в хвосте
            col.changeTimer += dt;
            if (col.changeTimer >= col.changeInterval)
            {
                col.changeTimer = 0f;

                // Меняем случайный символ в хвосте (не голову)
                if (col.texts.Count > 1)
                {
                    int randIdx = Random.Range(1, col.texts.Count);
                    if (col.texts[randIdx] != null)
                        col.texts[randIdx].text =
                            MatrixChars[Random.Range(0, MatrixChars.Length)];
                }
            }
        }
    }

    private void PulseVignette()
    {
        if (vignetteImage == null) return;
        vignetteTime += Time.deltaTime * 0.3f;
        Color c = vignetteColor;
        c.a = Mathf.Lerp(0.6f, 0.8f,
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