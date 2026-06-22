using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{
    [Header("Colors")]
    public Color topColor =
    new Color32(255, 170, 90, 255); // теплый оранжевый

    public Color middleColor =
        new Color32(255, 120, 70, 255); // м€гкий коралловый

    public Color bottomColor =
        new Color32(20, 10, 10, 255); // почти черный с теплым оттенком

    [Header("Animation")]
    public float animationSpeed = 0.2f;
    public float colorIntensity = 0.15f;

    private Texture2D texture;
    private Image image;

    private void Start()
    {
        CreateBackground();
    }

    private void Update()
    {
        UpdateGradient();
    }

    private void CreateBackground()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (!canvas)
            return;

        GameObject bg = new GameObject("Gradient");

        bg.transform.SetParent(canvas.transform, false);
        bg.transform.SetAsFirstSibling();

        RectTransform rect = bg.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        image = bg.AddComponent<Image>();

        texture = new Texture2D(1, 512, TextureFormat.RGBA32, false);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        image.sprite = sprite;

        UpdateGradient();
    }

    private void UpdateGradient()
    {
        float pulse = Mathf.Sin(Time.time * animationSpeed);

        Color animatedTop =
      Color.Lerp(
          new Color32(255, 170, 90, 255),
          new Color32(255, 210, 130, 255),
          (Mathf.Sin(Time.time * animationSpeed) + 1f) * 0.5f
      );

        Color animatedMiddle =
            Color.Lerp(
                new Color32(255, 120, 70, 255),
                new Color32(255, 150, 100, 255),
                (Mathf.Sin(Time.time * animationSpeed * 0.8f) + 1f) * 0.5f
            );

        for (int y = 0; y < texture.height; y++)
        {
            float t = y / (float)(texture.height - 1);

            Color color;

            if (t < 0.5f)
            {
                color = Color.Lerp(bottomColor, animatedMiddle, t * 2f);
            }
            else
            {
                color = Color.Lerp(animatedMiddle, animatedTop, (t - 0.5f) * 2f);
            }

            texture.SetPixel(0, y, color);
        }

        texture.Apply(false);
    }

    private void OnDestroy()
    {
        if (texture != null)
            Destroy(texture);
    }
}