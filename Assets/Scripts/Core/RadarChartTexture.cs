using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChartTexture : MonoBehaviour
{
    private const int AXIS_COUNT = 10;

    [Header("Settings")]
    public int textureSize = 1024;
    public float radius = 400f;
    public Color fillColor = new Color(0.2f, 0.6f, 1f, 0.6f);
    public Color axisColor = Color.white;
    public Color gridColor = new Color(1f, 1f, 1f, 0.25f);
    public Color bgColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    [Range(1, 8)] public int lineThickness = 2;
    [Range(1, 8)] public int axisThickness = 2;

    [Header("UI")]
    public Image chartImage;
    public TextMeshProUGUI[] axisLabels;

    [Header("Axis Names (optional)")]
    [SerializeField]
    private string[] axisNames = new string[AXIS_COUNT]
    {
        "Технический","Гуманитарный","Управленческий","Рабочий","Интроверт",
        "Экстраверт","Аналитик","Интуитивный","Стабильность","Открытость"
    };

    private Texture2D texture;
    private Color[] pixelBuffer;
    private float[] values = new float[AXIS_COUNT];

    private void Awake()
    {
        CreateTexture();
    }

    private void OnValidate()
    {
        if (textureSize < 64) textureSize = 64;
        if (radius <= 0) radius = textureSize * 0.4f;
    }

    private void CreateTexture()
    {
        if (texture != null)
        {
            DestroyImmediate(texture);
            texture = null;
        }

        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        pixelBuffer = new Color[textureSize * textureSize];

        if (chartImage != null)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), 100f);
            chartImage.sprite = sprite;
            chartImage.preserveAspect = true;
        }

        ClearBuffer();
        ApplyBuffer();
    }

    private void ClearBuffer()
    {
        for (int i = 0; i < pixelBuffer.Length; i++) pixelBuffer[i] = bgColor;
    }

    private void ApplyBuffer()
    {
        texture.SetPixels(pixelBuffer);
        texture.Apply();
    }

    private Vector2 TexCenter => new Vector2(textureSize * 0.5f, textureSize * 0.5f);

    public void UpdateChart(float[] newValues)
    {
        if (newValues == null) return;
        if (newValues.Length != AXIS_COUNT) return;

        float max = 0f;
        for (int i = 0; i < AXIS_COUNT; i++) if (newValues[i] > max) max = newValues[i];

        if (max > 0f)
        {
            for (int i = 0; i < AXIS_COUNT; i++) values[i] = Mathf.Clamp01(newValues[i] / max);
        }
        else
        {
            for (int i = 0; i < AXIS_COUNT; i++) values[i] = Mathf.Clamp01(newValues[i]);
        }

        DrawChart();
        UpdateLabels();
    }

    private void DrawChart()
    {
        ClearBuffer();

        Vector2 center = TexCenter;

        int rings = 5;
        for (int r = 1; r <= rings; r++)
        {
            float ringRadius = radius * (r / (float)rings);
            DrawCircle(center, ringRadius, gridColor, lineThickness);
        }

        for (int i = 0; i < AXIS_COUNT; i++)
        {
            float angle = Mathf.PI * 2f * i / AXIS_COUNT - Mathf.PI / 2f;
            Vector2 end = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            DrawLine(center, end, axisColor, axisThickness);
        }

        var points = new Vector2[AXIS_COUNT];
        for (int i = 0; i < AXIS_COUNT; i++)
        {
            float angle = Mathf.PI * 2f * i / AXIS_COUNT - Mathf.PI / 2f;
            float v = Mathf.Clamp01(values[i]);
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * v;
        }

        FillPolygon(points, fillColor);

        for (int i = 0; i < AXIS_COUNT; i++)
        {
            int next = (i + 1) % AXIS_COUNT;
            DrawLine(points[i], points[next], axisColor, Mathf.Max(1, axisThickness));
        }

        ApplyBuffer();
    }

    private void DrawLine(Vector2 a, Vector2 b, Color color, int thickness = 1)
    {
        int x0 = Mathf.RoundToInt(a.x);
        int y0 = Mathf.RoundToInt(a.y);
        int x1 = Mathf.RoundToInt(b.x);
        int y1 = Mathf.RoundToInt(b.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawFilledCircle(new Vector2(x0, y0), thickness, color);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void DrawCircle(Vector2 center, float r, Color color, int thickness = 1)
    {
        int steps = Mathf.Clamp(Mathf.RoundToInt(r * 2f), 64, 1024);
        Vector2 prev = center + new Vector2(r, 0f);
        for (int i = 1; i <= steps; i++)
        {
            float angle = Mathf.PI * 2f * i / steps;
            Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            DrawLine(prev, p, color, thickness);
            prev = p;
        }
    }

    private void DrawFilledCircle(Vector2 center, int radiusPx, Color color)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int r = Mathf.Max(0, radiusPx);

        int x0 = Mathf.Clamp(cx - r, 0, textureSize - 1);
        int x1 = Mathf.Clamp(cx + r, 0, textureSize - 1);
        int y0 = Mathf.Clamp(cy - r, 0, textureSize - 1);
        int y1 = Mathf.Clamp(cy + r, 0, textureSize - 1);

        int rr = r * r;
        for (int x = x0; x <= x1; x++)
        {
            int dx = x - cx;
            int dx2 = dx * dx;
            for (int y = y0; y <= y1; y++)
            {
                int dy = y - cy;
                if (dx2 + dy * dy <= rr)
                {
                    int idx = y * textureSize + x;
                    pixelBuffer[idx] = Blend(pixelBuffer[idx], color);
                }
            }
        }
    }

    private Color Blend(Color dst, Color src)
    {
        float a = src.a + dst.a * (1f - src.a);
        if (a <= 0f) return Color.clear;
        Color result = (src * src.a + dst * dst.a * (1f - src.a)) / a;
        result.a = a;
        return result;
    }

    private void FillPolygon(Vector2[] poly, Color color)
    {
        if (poly == null || poly.Length < 3) return;

        int minX = textureSize - 1, minY = textureSize - 1, maxX = 0, maxY = 0;
        for (int i = 0; i < poly.Length; i++)
        {
            int x = Mathf.RoundToInt(poly[i].x);
            int y = Mathf.RoundToInt(poly[i].y);
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }

        minX = Mathf.Clamp(minX, 0, textureSize - 1);
        minY = Mathf.Clamp(minY, 0, textureSize - 1);
        maxX = Mathf.Clamp(maxX, 0, textureSize - 1);
        maxY = Mathf.Clamp(maxY, 0, textureSize - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), poly))
                {
                    int idx = y * textureSize + x;
                    pixelBuffer[idx] = Blend(pixelBuffer[idx], color);
                }
            }
        }
    }

    private bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        int j = poly.Length - 1;
        for (int i = 0; i < poly.Length; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];
            bool intersect = ((pi.y > p.y) != (pj.y > p.y)) &&
                             (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y + Mathf.Epsilon) + pi.x);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    private void UpdateLabels()
    {
        if (axisLabels == null || axisLabels.Length == 0) return;

        for (int i = 0; i < axisLabels.Length && i < AXIS_COUNT; i++)
        {
            if (axisLabels[i] == null) continue;
            axisLabels[i].text = (i < axisNames.Length && !string.IsNullOrEmpty(axisNames[i])) ? axisNames[i] : $"Axis {i}";
        }
    }

    private void OnDestroy()
    {
        if (texture != null)
        {
            DestroyImmediate(texture);
            texture = null;
        }
    }

    public void SetTextureSize(int newSize)
    {
        if (newSize == textureSize) return;
        textureSize = Mathf.Max(32, newSize);
        CreateTexture();
        DrawChart();
    }
}
