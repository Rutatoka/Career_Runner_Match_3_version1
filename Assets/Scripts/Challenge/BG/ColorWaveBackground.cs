using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ColorWaveBackground : MonoBehaviour
{
    [Header("ѕалитра Ч бегуща€ волна по кругу")]
    public Color colorA = new Color(0.35f, 0.10f, 0.65f); // фиолетовый
    public Color colorB = new Color(0.20f, 0.20f, 0.85f); // синий
    public Color colorC = new Color(0.55f, 0.15f, 0.85f); // светло-фиолетовый
    public Color colorD = new Color(0.10f, 0.35f, 0.90f); // голубой-синий

    [Header("—корость")]
    [Tooltip("—колько секунд занимает полный цикл по всем 4 цветам")]
    public float cycleDuration = 6f;

    private Image targetImage;
    private Color[] palette;
    private float t;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        palette = new[] { colorA, colorB, colorC, colorD };
    }

    private void Update()
    {
        if (targetImage == null || palette == null || palette.Length == 0) return;
        if (cycleDuration <= 0f) return;

        t += Time.deltaTime / cycleDuration;
        if (t >= 1f) t -= 1f;

        targetImage.color = SampleWave(t);
    }

    // Ѕегуща€ волна Ч плавно идЄм от текущего цвета к следующему
    // по всей палитре, замыка€ круг в конце
    private Color SampleWave(float normalizedT)
    {
        int count = palette.Length;
        float scaled = normalizedT * count;

        int indexA = Mathf.FloorToInt(scaled) % count;
        int indexB = (indexA + 1) % count;
        float localT = scaled - Mathf.Floor(scaled);

        return Color.Lerp(palette[indexA], palette[indexB], localT);
    }
}