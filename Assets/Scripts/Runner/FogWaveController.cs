using UnityEngine;

public class FogWaveController : MonoBehaviour
{
    [Header("Материал тумана")]
    public Material fogMaterial;

    [Header("Размер плоскостей")]
    public float quadWidth = 30f;
    public float quadHeight = 20f;
    public float heightAboveGround = 0.3f;

    [Header("Скорость скроллинга")]
    public float layer1SpeedX = 0.01f;
    public float layer1SpeedZ = 0.008f;
    public float layer2SpeedX = -0.007f;
    public float layer2SpeedZ = 0.012f;
    public float layer3SpeedX = 0.005f;
    public float layer3SpeedZ = -0.006f;

    [Header("Волны")]
    public float waveAmplitude = 0.15f;
    public float waveSpeed = 0.5f;

    [Header("Цвет")]
    public float colorLerpSpeed = 2f;

    [Header("Следование за игроком")]
    public Transform playerTransform;
    public float followSpeed = 10f;

    private static readonly Color[] ProfessionColors = new Color[]
    {
        new Color(0.00f, 0.48f, 1.00f),
        new Color(0.69f, 0.32f, 0.87f),
        new Color(1.00f, 0.58f, 0.00f),
        new Color(0.20f, 0.78f, 0.35f),
        new Color(1.00f, 0.18f, 0.33f),
        new Color(1.00f, 0.80f, 0.00f),
        new Color(1.00f, 0.23f, 0.19f),
    };

    private static readonly Color ColorDefault = new Color(0.6f, 0.4f, 0.9f);

    private GameObject layer1;
    private GameObject layer2;
    private GameObject layer3;

    private Material mat1;
    private Material mat2;
    private Material mat3;

    private Vector2 offset1;
    private Vector2 offset2;
    private Vector2 offset3;

    private Color currentColor;
    private Color targetColor;

    // Убираем проверку fogMaterial в Start() — заменяем весь Start() на:
    private void Start()
    {
        if (playerTransform == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }

        // Сразу ставим FogSystem туда где игрок
        if (playerTransform != null)
            transform.position = new Vector3(0f, 0f, playerTransform.position.z);

        currentColor = ColorDefault;
        targetColor = ColorDefault;

        // Генерируем текстуру ДО создания материала
        Texture2D noiseTex = GenerateNoiseTexture(256, 256);

        if (fogMaterial == null)
        {
            fogMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            fogMaterial.SetFloat("_Surface", 1f);
            fogMaterial.renderQueue = 3000;
            fogMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fogMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fogMaterial.SetInt("_ZWrite", 0);
            fogMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        // Текстуру назначаем ДО клонирования
        fogMaterial.SetTexture("_BaseMap", noiseTex);
        fogMaterial.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.3f));

        // Теперь создаём слои — инстансы унаследуют текстуру
        layer1 = CreateFogLayer("FogLayer1", 0f, 0f);
        layer2 = CreateFogLayer("FogLayer2", 0.3f, 5f);
        layer3 = CreateFogLayer("FogLayer3", -0.2f, -5f);

        mat1 = layer1.GetComponent<Renderer>().material;
        mat2 = layer2.GetComponent<Renderer>().material;
        mat3 = layer3.GetComponent<Renderer>().material;

        // Явно прописываем текстуру на каждый инстанс
        mat1.SetTexture("_BaseMap", noiseTex);
        mat2.SetTexture("_BaseMap", noiseTex);
        mat3.SetTexture("_BaseMap", noiseTex);

        mat1.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        mat2.SetTextureScale("_BaseMap", new Vector2(1.5f, 3f));
        mat3.SetTextureScale("_BaseMap", new Vector2(3f, 1.5f));
    }
    private Texture2D GenerateNoiseTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;

        float offsetX = Random.Range(0f, 100f);
        float offsetY = Random.Range(0f, 100f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Несколько октав Perlin noise для мягкости
                float nx = (float)x / width;
                float ny = (float)y / height;

                float noise =
                    Mathf.PerlinNoise(nx * 3f + offsetX, ny * 3f + offsetY) * 0.5f +
                    Mathf.PerlinNoise(nx * 6f + offsetX, ny * 6f + offsetY) * 0.3f +
                    Mathf.PerlinNoise(nx * 12f + offsetX, ny * 12f + offsetY) * 0.2f;

                // Сглаживаем края — делаем более облачным
                noise = Mathf.Pow(noise, 1.5f);
                noise = Mathf.Clamp01(noise);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, noise));
            }
        }

        tex.Apply();
        return tex;
    }
    private void Update()
    {
        if (layer1 == null) return;

        UpdateTargetColor();

        // Цвет мгновенно без смешивания
        currentColor = targetColor;

        ScrollLayers();
        WaveLayers();
        ApplyColors();
        FollowPlayer();
    }

    private void ScrollLayers()
    {
        float dt = Time.deltaTime;

        offset1 += new Vector2(layer1SpeedX, layer1SpeedZ) * dt;
        offset2 += new Vector2(layer2SpeedX, layer2SpeedZ) * dt;
        offset3 += new Vector2(layer3SpeedX, layer3SpeedZ) * dt;

        mat1.SetTextureOffset("_BaseMap", offset1);
        mat2.SetTextureOffset("_BaseMap", offset2);
        mat3.SetTextureOffset("_BaseMap", offset3);
    }

    private void WaveLayers()
    {
        float time = Time.time;

        // Каждый слой плавно покачивается по высоте
        float y1 = heightAboveGround +
            Mathf.Sin(time * waveSpeed) * waveAmplitude;
        float y2 = heightAboveGround +
            Mathf.Sin(time * waveSpeed + 1f) * waveAmplitude;
        float y3 = heightAboveGround +
            Mathf.Sin(time * waveSpeed + 2f) * waveAmplitude;

        SetLayerY(layer1, y1);
        SetLayerY(layer2, y2);
        SetLayerY(layer3, y3);
    }

    private void ApplyColors()
    {
        // Каждый слой чуть разной прозрачности для глубины
        ApplyLayerColor(mat1, currentColor, 0.25f);
        ApplyLayerColor(mat2, currentColor, 0.18f);
        ApplyLayerColor(mat3, currentColor, 0.20f);
    }

    private void ApplyLayerColor(Material mat, Color color, float alpha)
    {
        Color c = color;
        c.a = alpha;
        mat.SetColor("_BaseColor", c);
    }

    private void FollowPlayer()
    {
        if (playerTransform == null) return;

        // Туман всегда следует за игроком по Z
        Vector3 target = new Vector3(
            0f,
            transform.position.y,
            playerTransform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            Time.deltaTime * followSpeed
        );
    }

    private void SetLayerY(GameObject layer, float y)
    {
        if (layer == null) return;
        var pos = layer.transform.localPosition;
        pos.y = y;
        layer.transform.localPosition = pos;
    }

    private GameObject CreateFogLayer(string name, float yOffset, float zOffset)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, heightAboveGround + yOffset, zOffset);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(quadWidth, quadHeight, 1f);

        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = CreateQuadMesh();

        var mr = go.AddComponent<MeshRenderer>();
        // Инстанс материала — не трогает оригинал
        mr.material = new Material(fogMaterial);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }

    private Mesh CreateQuadMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f, -0.5f),
            new Vector3( 0.5f, 0f,  0.5f),
            new Vector3(-0.5f, 0f,  0.5f),
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        return mesh;
    }

    private void UpdateTargetColor()
    {
        if (ProfessionSystem.Instance == null)
        {
            targetColor = ColorDefault;
            return;
        }

        ProfessionType type = ProfessionSystem.Instance.GetDominantType();

        if (type == ProfessionType.None)
        {
            targetColor = ColorDefault;
            return;
        }

        int index = (int)type;
        targetColor = index >= 0 && index < ProfessionColors.Length
            ? ProfessionColors[index]
            : ColorDefault;
    }
}