using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ColorWaveBackground : MonoBehaviour
{
    [Header("Настройки волны")]
    [Tooltip("Сколько секунд занимает полный цикл")]
    public float cycleDuration = 6f;

    [Header("Насыщенность и яркость волны")]
    [Range(0f, 1f)]
    public float saturationVariation = 0.3f;
    [Range(0f, 1f)]
    public float brightnessVariation = 0.2f;

    private Image targetImage;
    private Color[] palette;
    private float t;

    // Запоминаем последнюю профессию
    private ProfessionType lastDominantProfession = ProfessionType.None;
    private Color lastBaseColor = Color.clear;
    private bool hasProfession = false;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        GeneratePaletteFromDominantProfession();
    }

    private void Update()
    {
        if (targetImage == null) return;
        if (cycleDuration <= 0f) return;

        UpdatePaletteIfNeeded();

        if (palette == null || palette.Length == 0) return;

        t += Time.deltaTime / cycleDuration;
        if (t >= 1f) t -= 1f;

        targetImage.color = SampleWave(t);
    }

    private void UpdatePaletteIfNeeded()
    {
        ProfessionType currentType = ProfessionType.None;

        if (ProfessionSystem.Instance != null)
        {
            currentType = ProfessionSystem.Instance.GetDominantType();
        }

        // Если собрана профессия — запоминаем
        if (currentType != ProfessionType.None)
        {
            lastDominantProfession = currentType;
            hasProfession = true;
        }

        // Используем последнюю собранную, а не текущую
        ProfessionType typeToUse = hasProfession ? lastDominantProfession : ProfessionType.None;

        Color baseColor = Color.clear;
        if (typeToUse != ProfessionType.None)
        {
            ProfessionObjectData data = GetProfessionObjectData(typeToUse);
            if (data != null)
                baseColor = data.directionColor;
        }

        // Если цвет изменился — перегенерируем палитру
        if (baseColor != lastBaseColor)
        {
            lastBaseColor = baseColor;
            GeneratePaletteFromBaseColor(baseColor);
        }
    }

    private void GeneratePaletteFromDominantProfession()
    {
        Color baseColor = Color.white;

        if (ProfessionSystem.Instance != null)
        {
            ProfessionType type = ProfessionSystem.Instance.GetDominantType();
            if (type != ProfessionType.None)
            {
                lastDominantProfession = type;
                hasProfession = true;
                ProfessionObjectData data = GetProfessionObjectData(type);
                if (data != null)
                    baseColor = data.directionColor;
            }
        }

        GeneratePaletteFromBaseColor(baseColor);
    }

    private void GeneratePaletteFromBaseColor(Color baseColor)
    {
        if (baseColor == Color.clear || baseColor == Color.white)
        {
            baseColor = new Color(0.5f, 0.2f, 0.8f);
        }

        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        palette = new Color[4];

        for (int i = 0; i < 4; i++)
        {
            float hueShift = (i / 4f) * 0.15f;
            float hue = (h + hueShift) % 1f;
            float sat = Mathf.Clamp01(s + (i % 2 == 0 ? saturationVariation : -saturationVariation));
            float val = Mathf.Clamp01(v + (i < 2 ? brightnessVariation : -brightnessVariation));

            palette[i] = Color.HSVToRGB(hue, sat, val);
        }
    }

    private Color SampleWave(float normalizedT)
    {
        int count = palette.Length;
        float scaled = normalizedT * count;

        int indexA = Mathf.FloorToInt(scaled) % count;
        int indexB = (indexA + 1) % count;
        float localT = scaled - Mathf.Floor(scaled);

        return Color.Lerp(palette[indexA], palette[indexB], localT);
    }

    public void ResetToDefault()
    {
        lastDominantProfession = ProfessionType.None;
        hasProfession = false;
        lastBaseColor = Color.clear;
        GeneratePaletteFromBaseColor(new Color(0.5f, 0.2f, 0.8f));
    }

    private ProfessionObjectData GetProfessionObjectData(ProfessionType type)
    {
        if (ChallengeManager.Instance != null)
        {
            var objects = ChallengeManager.Instance.professionObjects;
            if (objects != null)
            {
                foreach (var obj in objects)
                {
                    if (obj != null && obj.professionType == type)
                        return obj;
                }
            }
        }

        var allData = Resources.LoadAll<ProfessionObjectData>("");
        foreach (var data in allData)
        {
            if (data.professionType == type)
                return data;
        }

        return null;
    }
}