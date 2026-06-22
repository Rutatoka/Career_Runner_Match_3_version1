using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MediaBackgroundController : MonoBehaviour
{
    public static MediaBackgroundController Instance { get; private set; }

    [Header("Canvas")]
    public Canvas targetCanvas;

    [Header("÷вета")]
    public Color backgroundColor = new Color(0.03f, 0.01f, 0.08f);
    public Color vignetteColor = new Color(0.0f, 0.0f, 0.05f, 0.7f);

    [Header("Ёквалайзер")]
    public int barCount = 20;
    public float barMaxHeight = 280f;
    public float barMinHeight = 8f;
    public float barDecaySpeed = 3.5f;
    public float barAttackSpeed = 18f;

    [Header("¬олны на нажатие")]
    public int ringCount = 4;
    public float ringExpandSpeed = 420f;
    public float ringMaxSize = 700f;
    private Sprite ringSprite;
    private static readonly Color[] HitColors = new Color[]
    {
        new Color(0.8f, 0.2f, 1.0f),
        new Color(0.2f, 0.7f, 1.0f),
        new Color(1.0f, 0.3f, 0.6f),
        new Color(0.3f, 1.0f, 0.7f),
    };

    private static readonly Color[] BarColors = new Color[]
    {
        new Color(0.7f, 0.1f, 1.0f, 0.55f),
        new Color(0.9f, 0.2f, 0.5f, 0.50f),
        new Color(0.2f, 0.6f, 1.0f, 0.50f),
        new Color(0.1f, 0.9f, 0.6f, 0.45f),
    };
    private bool isReady = false;
    private RectTransform canvasRect;
    private Image vignetteImage;
    private float vignetteTime;

    private struct BarData
    {
        public RectTransform rect;
        public Image image;
        public float currentHeight;
        public float targetHeight;
        public bool isTop;
    }

    private struct RingData
    {
        public RectTransform rect;
        public Image image;
        public bool active;
        public float currentSize;
        public Color color;
    }

    private BarData[] bottomBars;
    private BarData[] topBars;
    private RingData[] rings;
    private int ringPoolIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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

        CreateEqualizer();
        CreateRingPool();
        CreateVignette();
        CreateBackground();
        StartCoroutine(IdleAnimation());
        isReady = true;
    }

    private void Update()
    {
        if (!isReady) return;
        UpdateBars();
        UpdateRings();
        PulseVignette();
    }

    // ??? ѕ”ЅЋ»„Ќџ… API Ч вызываетс€ из ChallengeMediaController ??????

    // ћен€ем сигнатуры публичных методов:

    public void OnHit(int lane, string rating, Vector2 canvasPosition)
    {
        Color col = rating == "Perfect" ? new Color(0.0f, 1.0f, 0.3f)
                  : rating == "Good" ? new Color(1.0f, 0.85f, 0.0f)
                  : new Color(1.0f, 0.2f, 0.2f);

        float boost = rating == "Perfect" ? 1.0f
                    : rating == "Good" ? 0.7f
                    : 0.4f;

        if (bottomBars != null)
        {
            int center = Mathf.RoundToInt((float)lane / 3f * (barCount - 1));
            for (int i = 0; i < barCount; i++)
            {
                float dist = Mathf.Abs(i - center);
                float power = Mathf.Max(0f, 1f - dist / (barCount * 0.35f));
                float h = barMaxHeight * boost * power *
                    Random.Range(0.6f, 1.0f);

                if (h > bottomBars[i].currentHeight)
                    bottomBars[i].targetHeight = h;
                if (topBars != null && h > topBars[i].currentHeight)
                    topBars[i].targetHeight = h;
            }
        }

        SpawnRing(col, canvasPosition);
    }

    // Miss Ч без кольца совсем, только тихий всплеск баров
    public void OnMiss(int lane)
    {
        if (bottomBars == null) return;
        int center = Mathf.RoundToInt((float)lane / 3f * (barCount - 1));
        for (int i = 0; i < barCount; i++)
        {
            float dist = Mathf.Abs(i - center);
            float power = Mathf.Max(0f, 1f - dist / (barCount * 0.2f));
            float h = barMaxHeight * 0.15f * power;
            if (h > bottomBars[i].currentHeight)
                bottomBars[i].targetHeight = h;
        }
        // Ќикакого кольца при промахе Ч так и просили
    }

    // ??? ‘ќЌ ?????????????????????????????????????????????????????????
    private void CreateBackground()
    {
        var obj = CreateUIObject("BG_Background", targetCanvas.transform, -200);
        obj.AddComponent<Image>().color = backgroundColor;
        StretchFull(obj.GetComponent<RectTransform>());
    }

    // ??? Ё ¬јЋј…«≈– (снизу и сверху) ?????????????????????????????????
    private void CreateEqualizer()
    {
        float w = GetW();
        float h = GetH();

        float totalPad = w * 0.04f;
        float barW = (w - totalPad * 2f) / barCount - 2f;

        bottomBars = new BarData[barCount];
        topBars = new BarData[barCount];

        var bottomLayer = CreateUIObject("BG_EqBottom", targetCanvas.transform, -188);
        StretchFull(bottomLayer.GetComponent<RectTransform>());

        var topLayer = CreateUIObject("BG_EqTop", targetCanvas.transform, -187);
        StretchFull(topLayer.GetComponent<RectTransform>());

        for (int i = 0; i < barCount; i++)
        {
            float xPos = totalPad + i * (barW + 2f) + barW * 0.5f - w * 0.5f;
            Color col = BarColors[i % BarColors.Length];

            // —низу вверх
            bottomBars[i] = CreateBar(
                $"BarB_{i}", bottomLayer.transform,
                xPos, -h * 0.5f, barW, col, false);

            // —верху вниз (зеркально)
            topBars[i] = CreateBar(
                $"BarT_{i}", topLayer.transform,
                xPos, h * 0.5f, barW, col, true);
        }
    }

    private BarData CreateBar(string name, Transform parent,
        float x, float anchorY, float width, Color color, bool isTop)
    {
        var obj = CreateUIObject(name, parent, 0);
        var img = obj.AddComponent<Image>();
        img.color = color;

        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, isTop ? 1f : 0f);
        rect.sizeDelta = new Vector2(width, barMinHeight);
        rect.anchoredPosition = new Vector2(x, anchorY);

        return new BarData
        {
            rect = rect,
            image = img,
            currentHeight = barMinHeight,
            targetHeight = barMinHeight,
            isTop = isTop
        };
    }

    // ???  ќЋ№÷ј-¬ќЋЌџ ????????????????????????????????????????????????
    private Sprite CreateRingTexture()
    {
        int size = 128;
        int center = size / 2;
        int outerR = size / 2 - 2;
        int innerR = outerR - 10;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(center, center));

                if (dist <= outerR && dist >= innerR)
                {
                    // ћ€гкие кра€ кольца через smoothstep
                    float outerAlpha = Mathf.SmoothStep(
                        outerR, outerR - 3f, dist);
                    float innerAlpha = Mathf.SmoothStep(
                        innerR, innerR + 3f, dist);
                    float alpha = Mathf.Min(outerAlpha, innerAlpha);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f));
    }

    private void CreateRingPool()
    {
        // √енерируем спрайт кольца один раз
        ringSprite = CreateRingTexture();

        var layer = CreateUIObject("BG_Rings", targetCanvas.transform, -185);
        StretchFull(layer.GetComponent<RectTransform>());

        rings = new RingData[ringCount];
        for (int i = 0; i < ringCount; i++)
        {
            var obj = CreateUIObject($"Ring_{i}", layer.transform, 0);
            var img = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();

            // Ќазначаем спрайт кольца вместо сплошного пр€моугольника
            img.sprite = ringSprite;
            img.color = Color.clear;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            rings[i] = new RingData
            {
                rect = rect,
                image = img,
                active = false
            };
        }
    }
    private void SpawnRing(Color color, Vector2 canvasPosition)
    {
        int idx = ringPoolIndex % ringCount;
        ringPoolIndex++;

        rings[idx].active = true;
        rings[idx].currentSize = 0f;
        rings[idx].color = color;
        rings[idx].rect.sizeDelta = Vector2.zero;
        // —тавим кольцо туда где нажал палец
        rings[idx].rect.anchoredPosition = canvasPosition;
    }

    // ??? ¬»Ќ№≈“ ј ????????????????????????????????????????????????????
    private void CreateVignette()
    {
        var obj = CreateUIObject("BG_Vignette", targetCanvas.transform, -180);
        StretchFull(obj.GetComponent<RectTransform>());
        vignetteImage = obj.AddComponent<Image>();
        vignetteImage.color = vignetteColor;
    }

    // ??? UPDATE ??????????????????????????????????????????????????????
    private void UpdateBars()
    {
        if (bottomBars == null || topBars == null) return;
        float dt = Time.deltaTime;

        for (int i = 0; i < barCount; i++)
        {
            ref BarData b = ref bottomBars[i];
            ref BarData t = ref topBars[i];

            // јтака к цели, затем затухание
            if (b.currentHeight < b.targetHeight)
                b.currentHeight = Mathf.MoveTowards(
                    b.currentHeight, b.targetHeight, barAttackSpeed * barMaxHeight * dt);
            else
                b.currentHeight = Mathf.MoveTowards(
                    b.currentHeight, barMinHeight, barDecaySpeed * b.currentHeight * dt);

            b.targetHeight = Mathf.MoveTowards(
                b.targetHeight, barMinHeight, barDecaySpeed * b.targetHeight * dt);

            b.rect.sizeDelta = new Vector2(b.rect.sizeDelta.x, b.currentHeight);

            // «еркало сверху Ч чуть меньше
            t.currentHeight = b.currentHeight * 0.6f;
            t.rect.sizeDelta = new Vector2(t.rect.sizeDelta.x, t.currentHeight);

            // ÷вет зависит от высоты
            float energy = b.currentHeight / barMaxHeight;
            Color bc = b.image.color;
            bc.a = Mathf.Lerp(0.2f, 0.7f, energy);
            b.image.color = bc;
            t.image.color = bc;
        }
    }

    private void UpdateRings()
    {
        if (rings == null) return;
        float dt = Time.deltaTime;

        for (int i = 0; i < rings.Length; i++)
        {
            if (!rings[i].active) continue;

            rings[i].currentSize += ringExpandSpeed * dt;

            float t = rings[i].currentSize / ringMaxSize;
            float alpha = Mathf.Lerp(0.5f, 0f, t);

            Color c = rings[i].color;
            c.a = alpha;
            rings[i].image.color = c;
            rings[i].rect.sizeDelta =
                new Vector2(rings[i].currentSize, rings[i].currentSize);

            if (rings[i].currentSize >= ringMaxSize)
            {
                rings[i].active = false;
                rings[i].image.color = Color.clear;
                rings[i].rect.sizeDelta = Vector2.zero;
            }
        }
    }

    private void PulseVignette()
    {
        if (vignetteImage == null) return;
        vignetteTime += Time.deltaTime * 0.4f;
        Color c = vignetteColor;
        c.a = Mathf.Lerp(0.5f, 0.72f,
            (Mathf.Sin(vignetteTime) + 1f) * 0.5f);
        vignetteImage.color = c;
    }

    // ??? ‘ќЌќ¬јя јЌ»ћј÷»я ѕќ ќя ??????????????????????????????????????
    private IEnumerator IdleAnimation()
    {
        while (true)
        {
            // —лучайный тихий всплеск пока нет нажатий
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));

            if (bottomBars == null) continue;

            int idx = Random.Range(0, barCount);
            float h = Random.Range(barMaxHeight * 0.05f, barMaxHeight * 0.2f);
            if (h > bottomBars[idx].currentHeight)
                bottomBars[idx].targetHeight = h;
        }
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