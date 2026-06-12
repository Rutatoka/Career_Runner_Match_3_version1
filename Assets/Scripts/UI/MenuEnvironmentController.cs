//using UnityEngine;

//public class MenuEnvironmentController : MonoBehaviour
//{
//    [Header("Здания")]
//    public int buildingCount = 12;
//    public float cityRadius = 18f;
//    public float minBuildingHeight = 3f;
//    public float maxBuildingHeight = 14f;
//    public float minBuildingWidth = 1.2f;
//    public float maxBuildingWidth = 3f;

//    [Header("Неон")]
//    public float neonPulseSpeed = 1.2f;
//    public float neonPulseAmount = 0.4f;

//    [Header("Платформа игрока")]
//    public float platformRadius = 1.8f;
//    public float platformHeight = 0.08f;
//    public GameObject playerGround;

//    [Header("Туман")]
//    public Color fogColor = new Color(0.18f, 0.05f, 0.25f);
//    public float fogDensity = 0.04f;
//    [Header("Асфальтовая платформа (новое)")]
//    public float asphaltRoughness = 0.4f;
//    public float asphaltMetallic = 0.25f;
//    public Color asphaltBaseColor = new Color(0.12f, 0.12f, 0.14f);
//    public Color asphaltReflectionTint = new Color(0.2f, 0.05f, 0.3f, 0.3f);
//    [Header("Небо")]
//    public Camera mainCamera;
//    public Color skyColorTop = new Color(0.04f, 0.01f, 0.12f);
//    public Color skyColorBottom = new Color(0.15f, 0.05f, 0.35f);

//    private static readonly Color[] NeonColors = new Color[]
//    {
//        new Color(0.0f, 0.8f, 1.0f),    // циан
//        new Color(0.8f, 0.0f, 1.0f),    // фиолетовый
//        new Color(1.0f, 0.1f, 0.5f),    // розовый
//        new Color(0.1f, 1.0f, 0.6f),    // мятный
//        new Color(1.0f, 0.6f, 0.0f),    // оранжевый
//    };

//    private struct BuildingData
//    {
//        public Renderer bodyRenderer;
//        public Renderer[] neonRenderers;
//        public Color neonColor;
//        public float pulseOffset;
//        public MaterialPropertyBlock bodyBlock;
//        public MaterialPropertyBlock[] neonBlocks;
//    }

//    private BuildingData[] buildings;
//    private Material bodyMaterial;
//    private Material neonMaterial;
//    private Material platformMaterial;
//    private Material gradientMaterial;
//    private GameObject skyQuad;

//    private void Start()
//    {
//        SetupMaterials();
//        SetupFog();
//        SetupCamera();
//        SetupSkyGradient();
//        SetupPlatform();
//        SpawnBuildings();
//    }

//    private void Update()
//    {
//        PulseNeon();
//        ScrollSky();
//    }

//    private void SetupMaterials()
//    {
//        // Тёмный непрозрачный материал для зданий
//        bodyMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//        bodyMaterial.SetColor("_BaseColor", new Color(0.06f, 0.04f, 0.10f));
//        bodyMaterial.SetFloat("_Smoothness", 0.8f);
//        bodyMaterial.SetFloat("_Metallic", 0.3f);

//        // Неоновый светящийся материал
//        neonMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
//        neonMaterial.SetFloat("_Surface", 0f);

//        // Платформа
//        platformMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//        platformMaterial.SetColor("_BaseColor", new Color(0.08f, 0.05f, 0.15f));
//        platformMaterial.SetFloat("_Smoothness", 0.95f);
//        platformMaterial.SetFloat("_Metallic", 0.8f);
//        platformMaterial.EnableKeyword("_EMISSION");
//        platformMaterial.SetColor("_EmissionColor",
//            new Color(0.3f, 0.0f, 0.6f) * 0.8f);
//    }

//    private void SetupFog()
//    {
//        RenderSettings.fog = true;
//        RenderSettings.fogMode = FogMode.Exponential;
//        RenderSettings.fogColor = fogColor;
//        RenderSettings.fogDensity = fogDensity;
//    }

//    private void SetupCamera()
//    {
//        if (mainCamera == null)
//            mainCamera = Camera.main;
//        if (mainCamera != null)
//            mainCamera.backgroundColor = skyColorTop;
//    }

//    private void SetupSkyGradient()
//    {
//        // Большой Quad позади всего — имитирует градиентное небо
//        skyQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
//        skyQuad.name = "SkyGradient";
//        Destroy(skyQuad.GetComponent<Collider>());

//        skyQuad.transform.SetParent(transform);
//        skyQuad.transform.localPosition = new Vector3(0f, 8f, 25f);
//        skyQuad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
//        skyQuad.transform.localScale = new Vector3(60f, 30f, 1f);

//        gradientMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
//        gradientMaterial.SetColor("_BaseColor", skyColorBottom);
//        skyQuad.GetComponent<Renderer>().material = gradientMaterial;
//    }

//    private void SetupPlatform()
//    {
//        // Создаём материал мокрого асфальта
//        Material asphaltMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

//        // Основные свойства асфальта
//        asphaltMaterial.SetColor("_BaseColor", asphaltBaseColor);
//        asphaltMaterial.SetFloat("_Smoothness", 0.95f - asphaltRoughness); // 0.55 - влажный блеск
//        asphaltMaterial.SetFloat("_Metallic", asphaltMetallic);

//        // Включаем эмиссию для лёгкого отражения неона
//        asphaltMaterial.EnableKeyword("_EMISSION");
//        asphaltMaterial.SetColor("_EmissionColor", asphaltReflectionTint);

//        // Добавляем нормали для эффекта мокрой поверхности (если есть карта)
//        // asphaltMaterial.SetTexture("_BumpMap", yourNormalMap); // если у вас есть текстура асфальта

//        // Создаём платформу с небольшим смещением вниз, чтобы выглядеть как пол
//        if (playerGround != null)
//        {
//            var rend = playerGround.GetComponent<Renderer>();
//            if (rend != null)
//                rend.material = asphaltMaterial;
//            return;
//        }

//        // Создаём новую платформу
//        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//        platform.name = "AsphaltPlatform";
//        platform.transform.SetParent(transform);
//        platform.transform.localPosition = new Vector3(0f, -0.02f, 0f); // чуть ниже уровня глаз
//        platform.transform.localScale =
//            new Vector3(platformRadius * 2.2f, platformHeight * 0.5f, platformRadius * 2.2f);

//        Destroy(platform.GetComponent<Collider>());
//        platform.GetComponent<Renderer>().material = asphaltMaterial;

//        // Добавляем второй слой - тонкое зеркальное покрытие для эффекта воды
//        CreateWaterLayer();
//    }

//    private void CreateWaterLayer()
//    {
//        // Тонкий слой воды поверх асфальта для мокрого эффекта
//        GameObject waterLayer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//        waterLayer.name = "WaterReflection";
//        waterLayer.transform.SetParent(transform);
//        waterLayer.transform.localPosition = new Vector3(0f, 0.01f, 0f);
//        waterLayer.transform.localScale =
//            new Vector3(platformRadius * 2.1f, 0.01f, platformRadius * 2.1f);

//        Destroy(waterLayer.GetComponent<Collider>());

//        Material waterMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//        waterMaterial.SetColor("_BaseColor", new Color(0.02f, 0.02f, 0.04f, 0.3f));
//        waterMaterial.SetFloat("_Smoothness", 0.99f);
//        waterMaterial.SetFloat("_Metallic", 0.0f);
//        waterMaterial.EnableKeyword("_EMISSION");
//        waterMaterial.SetColor("_EmissionColor", new Color(0.05f, 0.02f, 0.15f, 0.2f));

//        waterLayer.GetComponent<Renderer>().material = waterMaterial;
//    }

//    private void SpawnBuildings()
//    {
//        buildings = new BuildingData[buildingCount];

//        for (int i = 0; i < buildingCount; i++)
//        {
//            // Распределяем здания только в диапазоне 60-300 градусов
//            // Оставляем 0-60 и 300-360 пустыми — это зона перед камерой
//            float t = (float)i / buildingCount;
//            float angle = Mathf.Lerp(70f, 290f, t) + Random.Range(-8f, 8f);

//            float dist = cityRadius + Random.Range(-2f, 4f);
//            float rad = angle * Mathf.Deg2Rad;

//            float x = Mathf.Sin(rad) * dist;
//            float z = Mathf.Cos(rad) * dist;

//            float h = Random.Range(minBuildingHeight, maxBuildingHeight);
//            float w = Random.Range(minBuildingWidth, maxBuildingWidth);
//            float d = Random.Range(minBuildingWidth, maxBuildingWidth);

//            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
//            body.name = $"Building_{i}";
//            body.transform.SetParent(transform);
//            body.transform.localPosition = new Vector3(x, h * 0.5f, z);
//            body.transform.localScale = new Vector3(w, h, d);
//            body.transform.localRotation =
//                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
//            Destroy(body.GetComponent<Collider>());

//            var bodyRend = body.GetComponent<Renderer>();
//            bodyRend.material = new Material(bodyMaterial);

//            int neonCount = Random.Range(2, 4);
//            var neonRends = new Renderer[neonCount];
//            var neonBlocks = new MaterialPropertyBlock[neonCount];
//            Color neonCol = NeonColors[Random.Range(0, NeonColors.Length)];

//            for (int n = 0; n < neonCount; n++)
//            {
//                var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
//                strip.name = $"Neon_{i}_{n}";
//                strip.transform.SetParent(body.transform);
//                Destroy(strip.GetComponent<Collider>());

//                float neonY = Random.Range(-0.3f, 0.4f);
//                strip.transform.localPosition = new Vector3(0f, neonY, 0.51f);
//                strip.transform.localScale = new Vector3(1f, 0.04f / h, 0.02f);

//                var nRend = strip.GetComponent<Renderer>();
//                nRend.material = new Material(neonMaterial);
//                neonRends[n] = nRend;
//                neonBlocks[n] = new MaterialPropertyBlock();
//            }

//            buildings[i] = new BuildingData
//            {
//                bodyRenderer = bodyRend,
//                neonRenderers = neonRends,
//                neonColor = neonCol,
//                pulseOffset = Random.Range(0f, Mathf.PI * 2f),
//                bodyBlock = new MaterialPropertyBlock(),
//                neonBlocks = neonBlocks
//            };
//        }
//    }

//    private void PulseNeon()
//    {
//        float time = Time.time;

//        foreach (var b in buildings)
//        {
//            if (b.neonRenderers == null) continue;

//            float pulse = neonPulseAmount +
//                (1f - neonPulseAmount) *
//                ((Mathf.Sin(time * neonPulseSpeed + b.pulseOffset) + 1f) * 0.5f);

//            Color emissive = b.neonColor * pulse * 2.5f;

//            for (int n = 0; n < b.neonRenderers.Length; n++)
//            {
//                if (b.neonRenderers[n] == null) continue;
//                b.neonRenderers[n].GetPropertyBlock(b.neonBlocks[n]);
//                b.neonBlocks[n].SetColor("_BaseColor", emissive);
//                b.neonRenderers[n].SetPropertyBlock(b.neonBlocks[n]);
//            }
//        }
//    }

//    private float skyScrollOffset;

//    private void ScrollSky()
//    {
//        // Лёгкое покачивание неба для живости
//        skyScrollOffset += Time.deltaTime * 0.02f;
//        if (skyQuad != null)
//        {
//            float sway = Mathf.Sin(skyScrollOffset) * 0.3f;
//            skyQuad.transform.localPosition =
//                new Vector3(sway, 8f + Mathf.Sin(skyScrollOffset * 0.5f) * 0.2f, 25f);
//        }
//    }

//    private void OnDestroy()
//    {
//        // Восстанавливаем туман при выходе из сцены
//        RenderSettings.fog = false;
//    }
//}// MenuEnvironmentController.cs — ПОЛНОСТЬЮ ПЕРЕРАБОТАННАЯ ВЕРСИЯ
// Удалите старый скрипт и замените этим.
// MenuEnvironmentController.cs — ВЕРСИЯ 3.0 (реалистичный асфальт)
// Удалите старый скрипт полностью, замените этим.
using UnityEngine;

public class MenuEnvironmentController : MonoBehaviour
{
    [Header("Здания")]
    public int buildingCount = 30;
    public float cityRadius = 12f; // уменьшил, чтобы были ближе
    public float minBuildingHeight = 8f;
    public float maxBuildingHeight = 30f;
    public float minBuildingWidth = 2f;
    public float maxBuildingWidth = 4f;

    [Header("Окна (неон)")]
    public float windowPulseSpeed = 2.5f;
    [Range(1f, 10f)] public float windowIntensity = 6f;

    [Header("Платформа игрока")]
    public GameObject playerGround;

    [Header("Туман")]
    public Color fogColor = new Color(0.12f, 0.06f, 0.18f);
    public float fogDensity = 0.025f;

    [Header("Асфальт")]
    public float platformRadius = 7f;
    public Color asphaltColor = new Color(0.13f, 0.13f, 0.15f);
    public float groundY = 3f; // асфальт на Y = 3

    [Header("Небо")]
    public Camera mainCamera;
    public Color skyColorTop = new Color(0.02f, 0.01f, 0.08f);
    public Color skyColorBottom = new Color(0.1f, 0.04f, 0.22f);

    private Material buildingMaterial;
    private GameObject skyQuad;

    private struct Building
    {
        public Renderer mainRenderer;
        public Renderer[] windowRenderers;
        public MaterialPropertyBlock[] windowBlocks;
        public Color windowColor;
        public float pulseOffset;
    }

    private Building[] buildings;

    private void Start()
    {
        SetupFog();
        SetupCamera();
        CreateSky();
        CreateGround();
        CreateBuildings();
    }

    private void Update()
    {
        PulseWindows();
        MoveSky();
    }

    private void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    private void SetupCamera()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) mainCamera.backgroundColor = skyColorTop;
    }

    private void CreateSky()
    {
        skyQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        skyQuad.name = "SkyGradient";
        Destroy(skyQuad.GetComponent<Collider>());
        skyQuad.transform.SetParent(transform);
        skyQuad.transform.localPosition = new Vector3(0f, 12f, 35f);
        skyQuad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        skyQuad.transform.localScale = new Vector3(80f, 45f, 1f);

        Material skyMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        skyMat.SetColor("_BaseColor", skyColorBottom);
        skyQuad.GetComponent<Renderer>().material = skyMat;
    }

    private void CreateGround()
    {
        if (playerGround != null) Destroy(playerGround);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "AsphaltGround";
        ground.transform.SetParent(transform);
        ground.transform.localPosition = new Vector3(0f, groundY, 0f);
        ground.transform.localScale = new Vector3(14f, 0.05f, 140f);
        Destroy(ground.GetComponent<Collider>());

        Material groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        groundMat.SetColor("_BaseColor", asphaltColor);
        groundMat.SetFloat("_Smoothness", 0.7f);
        groundMat.SetFloat("_Metallic", 0.1f);
        groundMat.EnableKeyword("_EMISSION");
        groundMat.SetColor("_EmissionColor", new Color(0.05f, 0.02f, 0.1f));
        ground.GetComponent<Renderer>().material = groundMat;

        playerGround = ground;
    }

    private void CreateBuildings()
    {
        buildings = new Building[buildingCount];
        Color[] windowColors = {
            new Color(0f, 0.9f, 1f),
            new Color(0.9f, 0f, 1f),
            new Color(1f, 0.2f, 0.6f),
            new Color(0.1f, 1f, 0.7f),
            new Color(1f, 0.7f, 0f)
        };

        buildingMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        buildingMaterial.SetColor("_BaseColor", new Color(0.05f, 0.03f, 0.08f));

        for (int i = 0; i < buildingCount; i++)
        {
            float t = (float)i / buildingCount;
            float angle = Mathf.Lerp(-60f, 60f, t); // ровная дуга, без разброса
            float radius = cityRadius + 20f; // фиксированное расстояние
                          float rad = angle * Mathf.Deg2Rad;                  // Чтобы здания не перекрывали друг друга, можно добавить небольшой сдвиг по X
            float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius + Random.Range(-0.5f, 0.5f);
            float z = Mathf.Cos(rad) * radius + Random.Range(-10f, 15f); // разброс по Z
       


            float height = Random.Range(minBuildingHeight, maxBuildingHeight);
            float width = Random.Range(minBuildingWidth, maxBuildingWidth);
            float depth = Random.Range(minBuildingWidth, maxBuildingWidth);

            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Building{i}";
            building.transform.SetParent(transform);
            building.transform.localPosition = new Vector3(x, groundY + height * 0.5f, z);
            building.transform.localScale = new Vector3(width, height, depth);
            building.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Destroy(building.GetComponent<Collider>());

            Renderer mainRend = building.GetComponent<Renderer>();
            mainRend.material = new Material(buildingMaterial);

            // Окна: 10-25 на здание
            int windowCount = Random.Range(10, 25);
            Renderer[] windowRenderers = new Renderer[windowCount];
            MaterialPropertyBlock[] windowBlocks = new MaterialPropertyBlock[windowCount];
            Color windowColor = windowColors[Random.Range(0, windowColors.Length)];

            for (int w = 0; w < windowCount; w++)
            {
                GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
                window.name = $"Window{i}_{w}";
                window.transform.SetParent(building.transform);
                Destroy(window.GetComponent<Collider>());

                // Случайная сторона
                float side = Random.Range(0, 4);
                float offsetX = 0f, offsetZ = 0f;
                if (side == 0) offsetZ = 0.5f;
                else if (side == 1) offsetZ = -0.5f;
                else if (side == 2) offsetX = -0.5f;
                else if (side == 3) offsetX = 0.5f;

                float windowY = Random.Range(-0.4f, 0.4f);
                float windowSize = Random.Range(0.06f, 0.12f);

                window.transform.localPosition = new Vector3(offsetX, windowY, offsetZ);
                window.transform.localScale = new Vector3(windowSize, windowSize * 1.5f, 0.02f);

                Renderer wRend = window.GetComponent<Renderer>();
                Material wMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                wMat.EnableKeyword("_EMISSION");
                wRend.material = wMat;
                windowRenderers[w] = wRend;
                windowBlocks[w] = new MaterialPropertyBlock();
            }

            buildings[i] = new Building
            {
                mainRenderer = mainRend,
                windowRenderers = windowRenderers,
                windowBlocks = windowBlocks,
                windowColor = windowColor,
                pulseOffset = Random.Range(0f, Mathf.PI * 2f)
            };
        }
    }

    private void PulseWindows()
    {
        float time = Time.time;
        foreach (var b in buildings)
        {
            float pulse = 0.4f + 0.6f * ((Mathf.Sin(time * windowPulseSpeed + b.pulseOffset) + 1f) * 0.5f);
            Color color = b.windowColor * pulse * windowIntensity;

            for (int w = 0; w < b.windowRenderers.Length; w++)
            {
                if (b.windowRenderers[w] == null) continue;
                b.windowRenderers[w].GetPropertyBlock(b.windowBlocks[w]);
                b.windowBlocks[w].SetColor("_EmissionColor", color);
                b.windowRenderers[w].SetPropertyBlock(b.windowBlocks[w]);
            }
        }
    }

    private void MoveSky()
    {
        if (skyQuad != null)
        {
            float sway = Mathf.Sin(Time.time * 0.01f) * 0.1f;
            skyQuad.transform.localPosition = new Vector3(sway, 12f + Mathf.Sin(Time.time * 0.005f) * 0.1f, 35f);
        }
    }

    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }
}