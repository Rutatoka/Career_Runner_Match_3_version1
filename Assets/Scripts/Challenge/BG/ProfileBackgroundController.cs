using UnityEngine;

public class ProfileBackgroundController : MonoBehaviour
{
    [Header("Камера")]
    public Camera mainCamera;

    [Header("Цвета")]
    public Color backgroundColor = new Color(0.05f, 0.02f, 0.10f);
    public Color wallColor = new Color(0.08f, 0.04f, 0.14f);
    public Color floorColor = new Color(0.06f, 0.03f, 0.11f);
    public Color cabinetColor = new Color(0.10f, 0.05f, 0.18f);
    public Color mirrorColor = new Color(0.15f, 0.08f, 0.25f);
    public Color neonPurple = new Color(0.7f, 0.1f, 1.0f);
    public Color neonPink = new Color(1.0f, 0.2f, 0.7f);
    public Color neonCyan = new Color(0.2f, 0.8f, 1.0f);

    [Header("Анимация")]
    public float neonPulseSpeed = 1.1f;
    public float neonPulseAmount = 0.35f;
    public float mirrorShimmerSpeed = 0.6f;

    private struct NeonStrip
    {
        public Renderer rend;
        public Color baseColor;
        public float pulseOffset;
        public float pulseSpeed;
        public MaterialPropertyBlock block;
    }

    private NeonStrip[] neonStrips;
    private Renderer mirrorRenderer;
    private float mirrorTime;
    private MaterialPropertyBlock mirrorBlock;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera != null)
            mainCamera.backgroundColor = backgroundColor;

        SetupFog();
        SetupLighting();
        BuildRoom();
    }

    private void Update()
    {
        PulseNeon();
        ShimmerMirror();
    }

    // ??? ТУМАН ???????????????????????????????????????????????????????
    private void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.06f, 0.02f, 0.12f);
        RenderSettings.fogDensity = 0.06f;
    }

    // ??? ОСВЕЩЕНИЕ ???????????????????????????????????????????????????
    private void SetupLighting()
    {
        RenderSettings.ambientLight = new Color(0.06f, 0.03f, 0.12f);

        // Основной свет над персонажем — чуть левее (у зеркала)
        var lightObj = new GameObject("ProfileLight_Main");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(-1f, 4f, 0f);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.7f, 0.3f, 1.0f);
        light.intensity = 2.0f;
        light.range = 10f;

        // Свет внутри шкафа
        var lightObj2 = new GameObject("ProfileLight_Wardrobe");
        lightObj2.transform.SetParent(transform);
        lightObj2.transform.localPosition = new Vector3(2.5f, 4f, 4f);
        var light2 = lightObj2.AddComponent<Light>();
        light2.type = LightType.Point;
        light2.color = new Color(1.0f, 0.3f, 0.7f);
        light2.intensity = 1.2f;
        light2.range = 5f;

        // Подсветка зеркала
        // Свет подсвечивающий зеркало сзади персонажа
        var lightObj3 = new GameObject("ProfileLight_Mirror");
        lightObj3.transform.SetParent(transform);
        lightObj3.transform.localPosition = new Vector3(-0.5f, 3f, 3f);
        var light3 = lightObj3.AddComponent<Light>();
        light3.type = LightType.Point;
        light3.color = new Color(0.6f, 0.2f, 1.0f);
        light3.intensity = 1.2f;
        light3.range = 7f;

    }

    // ??? КОМНАТА ?????????????????????????????????????????????????????
    // Замени BuildRoom() полностью:
    private void BuildRoom()
    {
        var strips = new System.Collections.Generic.List<NeonStrip>();

        BuildFloor();
        BuildBackWall();
        BuildMirror();
        BuildSideWalls();
        BuildCabinets(strips);
        BuildShelves(strips);
        BuildCeilingStrips(strips);

        neonStrips = strips.ToArray();
    }

    // Замени BuildFloor():
    private void BuildFloor()
    {
        var floor = CreatePrimitive(
            PrimitiveType.Cube, "Floor",
            new Vector3(0f, -0.05f, 1f),
            new Vector3(8f, 0.1f, 10f),
            floorColor);
        SetMaterialProps(floor.GetComponent<Renderer>(),
            floorColor, smoothness: 0.95f, metallic: 0.4f);

        for (int i = -2; i <= 2; i++)
        {
            CreatePrimitive(
                PrimitiveType.Cube, $"FloorLine_{i}",
                new Vector3(i * 1.2f, 0.01f, 1f),
                new Vector3(0.03f, 0.02f, 10f),
                neonPurple);
        }
    }

    // Замени BuildBackWall():
    private void BuildBackWall()
    {
        CreatePrimitive(
            PrimitiveType.Cube, "BackWall",
            new Vector3(0f, 3f, 6f),
            new Vector3(8f, 8f, 0.3f),
            wallColor);
    }

    // Замени BuildMirror():
    private void BuildMirror()
    {
        // Зеркало на ЗАДНЕЙ стене — за персонажем
        // Рамка — красивая широкая с закруглёнными краями имитируем кубами
        float cx = -0.5f;  // чуть левее центра — персонаж стоит по центру
        float cy = 2.5f;
        float cz = 5.7f;   // задняя стена

        // Рамка — тёмно-фиолетовая
        CreatePrimitive(
            PrimitiveType.Cube, "MirrorFrame",
            new Vector3(cx, cy, cz),
            new Vector3(2.8f, 5.0f, 0.12f),
            new Color(0.2f, 0.08f, 0.38f));

        // Зеркальная поверхность
        var mirror = CreatePrimitive(
            PrimitiveType.Cube, "Mirror",
            new Vector3(cx, cy, cz - 0.07f),
            new Vector3(2.3f, 4.5f, 0.05f),
            mirrorColor);

        var rend = mirror.GetComponent<Renderer>();
        SetMaterialProps(rend, mirrorColor, smoothness: 1.0f, metallic: 0.95f);
        mirrorRenderer = rend;
        mirrorBlock = new MaterialPropertyBlock();

        // Неоновая рамка — 4 полосы вокруг зеркала
        float hw = 2.3f * 0.5f;
        float hh = 4.5f * 0.5f;
        float nz = cz - 0.1f;
        float nt = 0.08f;

        // Верхняя полоса
        var t = CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Top",
            new Vector3(cx, cy + hh + nt * 0.5f, nz),
            new Vector3(2.3f + nt * 2f, nt, 0.06f),
            neonPurple);
        t.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        t.GetComponent<Renderer>().material.SetColor("_EmissionColor", neonPurple * 2.5f);

        // Нижняя полоса
        var b = CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Bot",
            new Vector3(cx, cy - hh - nt * 0.5f, nz),
            new Vector3(2.3f + nt * 2f, nt, 0.06f),
            neonPink);
        b.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        b.GetComponent<Renderer>().material.SetColor("_EmissionColor", neonPink * 2.5f);

        // Левая полоса
        var l = CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Left",
            new Vector3(cx - hw - nt * 0.5f, cy, nz),
            new Vector3(nt, 4.5f, 0.06f),
            neonPurple);
        l.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        l.GetComponent<Renderer>().material.SetColor("_EmissionColor", neonPurple * 2.5f);

        // Правая полоса
        var r = CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Right",
            new Vector3(cx + hw + nt * 0.5f, cy, nz),
            new Vector3(nt, 4.5f, 0.06f),
            neonPink);
        r.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        r.GetComponent<Renderer>().material.SetColor("_EmissionColor", neonPink * 2.5f);

        // Угловые акценты — маленькие квадратики в углах
        Vector3[] corners = {
        new Vector3(cx - hw, cy + hh, nz),
        new Vector3(cx + hw, cy + hh, nz),
        new Vector3(cx - hw, cy - hh, nz),
        new Vector3(cx + hw, cy - hh, nz),
    };
        foreach (var corner in corners)
        {
            var c = CreatePrimitive(PrimitiveType.Cube, "MirrorCorner",
                corner,
                new Vector3(nt * 2f, nt * 2f, 0.07f),
                neonCyan);
            c.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            c.GetComponent<Renderer>().material.SetColor(
                "_EmissionColor", neonCyan * 3f);
        }
    }
    private void AddNeonFrameVertical(
    Vector3 center, float w, float h, Color color)
    {
        float thickness = 0.07f;
        float depth = 0.05f;

        // Верх и низ — вытянуты по Z
        CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Top",
            center + new Vector3(0f, h * 0.5f, 0f),
            new Vector3(depth, thickness, w + thickness),
            color).GetComponent<Renderer>()
            .material.EnableKeyword("_EMISSION");

        CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Bot",
            center + new Vector3(0f, -h * 0.5f, 0f),
            new Vector3(depth, thickness, w + thickness),
            color).GetComponent<Renderer>()
            .material.EnableKeyword("_EMISSION");

        // Лево и право — вытянуты по Y
        CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Left",
            center + new Vector3(0f, 0f, -w * 0.5f),
            new Vector3(depth, h, thickness),
            color).GetComponent<Renderer>()
            .material.EnableKeyword("_EMISSION");

        CreatePrimitive(PrimitiveType.Cube, "MirrorNeon_Right",
            center + new Vector3(0f, 0f, w * 0.5f),
            new Vector3(depth, h, thickness),
            color).GetComponent<Renderer>()
            .material.EnableKeyword("_EMISSION");
    }
    // Замени BuildSideWalls():
    private void BuildSideWalls()
    {
        CreatePrimitive(
            PrimitiveType.Cube, "LeftWall",
            new Vector3(-4f, 3f, 1f),
            new Vector3(0.3f, 8f, 10f),
            wallColor);

        CreatePrimitive(
            PrimitiveType.Cube, "RightWall",
            new Vector3(4f, 3f, 1f),
            new Vector3(0.3f, 8f, 10f),
            wallColor);
    }

    // Замени BuildCabinets():
    private void BuildCabinets(System.Collections.Generic.List<NeonStrip> strips)
    {
        // Один большой открытый шкаф справа
        BuildOpenWardrobe(strips);
    }

    private void BuildOpenWardrobe(System.Collections.Generic.List<NeonStrip> strips)
    {
        // Корпус шкафа — правая сторона
        // Задняя стенка шкафа
        CreatePrimitive(
            PrimitiveType.Cube, "WardrobeBack",
            new Vector3(3.2f, 2.5f, 4.5f),
            new Vector3(0.1f, 6f, 4f),
            cabinetColor);

        // Верхняя стенка
        CreatePrimitive(
            PrimitiveType.Cube, "WardrobeTop",
            new Vector3(2.0f, 5.4f, 4.5f),
            new Vector3(2.4f, 0.15f, 4f),
            cabinetColor);

        // Нижняя стенка — пол шкафа
        CreatePrimitive(
            PrimitiveType.Cube, "WardrobeBottom",
            new Vector3(2.0f, -0.4f, 4.5f),
            new Vector3(2.4f, 0.15f, 4f),
            cabinetColor);

        // Верхняя перекладина для одежды
        var rod = CreatePrimitive(
            PrimitiveType.Cylinder, "WardrobeRod",
            new Vector3(2.0f, 4.5f, 3.0f),
            new Vector3(0.04f, 1.2f, 0.04f),
            new Color(0.5f, 0.25f, 0.8f));
        rod.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Вешалки на перекладине
        for (int h = 0; h < 4; h++)
        {
            CreatePrimitive(
                PrimitiveType.Cube, $"WardrobeHanger_{h}",
                new Vector3(2.0f, 4.1f, 2.5f + h * 0.55f),
                new Vector3(0.5f, 0.04f, 0.04f),
                new Color(0.6f, 0.3f, 0.9f));
        }

        // Неоновая полоска сверху шкафа
        var topNeon = CreatePrimitive(
            PrimitiveType.Cube, "WardrobeTopNeon",
            new Vector3(2.0f, 5.35f, 4.5f),
            new Vector3(2.4f, 0.04f, 4f),
            neonPink);
        topNeon.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        strips.Add(new NeonStrip
        {
            rend = topNeon.GetComponent<Renderer>(),
            baseColor = neonPink,
            pulseOffset = 0f,
            pulseSpeed = neonPulseSpeed,
            block = new MaterialPropertyBlock()
        });

        // Неоновая полоска снизу шкафа
        var botNeon = CreatePrimitive(
            PrimitiveType.Cube, "WardrobeBotNeon",
            new Vector3(2.0f, -0.35f, 4.5f),
            new Vector3(2.4f, 0.04f, 4f),
            neonPurple);
        botNeon.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        strips.Add(new NeonStrip
        {
            rend = botNeon.GetComponent<Renderer>(),
            baseColor = neonPurple,
            pulseOffset = 1f,
            pulseSpeed = neonPulseSpeed,
            block = new MaterialPropertyBlock()
        });
    }


    // Замени BuildShelves():
    private void BuildShelves(System.Collections.Generic.List<NeonStrip> strips)
    {
        // Полки внутри шкафа — три ряда справа
        float[] shelfZ = { 3.0f, 4.5f, 5.8f };
        float[] shelfHeights = { 0.8f, 2.0f, 3.2f };

        for (int i = 0; i < shelfHeights.Length; i++)
        {
            // Полка
            CreatePrimitive(
                PrimitiveType.Cube, $"Shelf_{i}",
                new Vector3(2.0f, shelfHeights[i], shelfZ[i]),
                new Vector3(2.2f, 0.07f, 0.9f),
                new Color(0.15f, 0.07f, 0.28f));

            // Подсветка под полкой
            var neonObj = CreatePrimitive(
                PrimitiveType.Cube, $"ShelfNeon_{i}",
                new Vector3(2.0f, shelfHeights[i] - 0.06f, shelfZ[i]),
                new Vector3(2.2f, 0.025f, 0.05f),
                i % 2 == 0 ? neonCyan : neonPurple);

            var rend = neonObj.GetComponent<Renderer>();
            rend.material.EnableKeyword("_EMISSION");

            strips.Add(new NeonStrip
            {
                rend = rend,
                baseColor = i % 2 == 0 ? neonCyan : neonPurple,
                pulseOffset = i * 0.8f,
                pulseSpeed = neonPulseSpeed,
                block = new MaterialPropertyBlock()
            });
        }
    }

    // Замени BuildCeilingStrips():
    private void BuildCeilingStrips(System.Collections.Generic.List<NeonStrip> strips)
    {
        CreatePrimitive(
            PrimitiveType.Cube, "Ceiling",
            new Vector3(0f, 6.5f, 1f),
            new Vector3(8f, 0.2f, 10f),
            wallColor);

        Color[] stripColors = { neonPurple, neonPink, neonCyan, neonPurple };
        float[] xPositions = { -2.5f, -0.8f, 0.8f, 2.5f };

        for (int i = 0; i < stripColors.Length; i++)
        {
            var neonObj = CreatePrimitive(
                PrimitiveType.Cube, $"CeilingStrip_{i}",
                new Vector3(xPositions[i], 6.3f, 1f),
                new Vector3(0.05f, 0.05f, 8f),
                stripColors[i]);

            var rend = neonObj.GetComponent<Renderer>();
            rend.material.EnableKeyword("_EMISSION");

            strips.Add(new NeonStrip
            {
                rend = rend,
                baseColor = stripColors[i],
                pulseOffset = i * 0.7f,
                pulseSpeed = neonPulseSpeed * (0.8f + i * 0.15f),
                block = new MaterialPropertyBlock()
            });
        }
    }
    // ??? НЕОНОВАЯ РАМКА ??????????????????????????????????????????????
    private void AddNeonFrame(Vector3 center, float w, float h, Color color)
    {
        float thickness = 0.08f;
        float depth = 0.05f;

        // Верх, низ, лево, право
        Vector3[] positions = {
            center + new Vector3(0f,  h * 0.5f, 0f),
            center + new Vector3(0f, -h * 0.5f, 0f),
            center + new Vector3(-w * 0.5f, 0f, 0f),
            center + new Vector3( w * 0.5f, 0f, 0f),
        };
        Vector3[] sizes = {
            new Vector3(w + thickness, thickness, depth),
            new Vector3(w + thickness, thickness, depth),
            new Vector3(thickness, h, depth),
            new Vector3(thickness, h, depth),
        };

        for (int i = 0; i < 4; i++)
        {
            var obj = CreatePrimitive(
                PrimitiveType.Cube, $"MirrorNeon_{i}",
                positions[i], sizes[i], color);
            var rend = obj.GetComponent<Renderer>();
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", color * 2f);
        }
    }

    // ??? UPDATE ??????????????????????????????????????????????????????
    private void PulseNeon()
    {
        if (neonStrips == null) return;
        float t = Time.time;

        foreach (var s in neonStrips)
        {
            if (s.rend == null) continue;

            float pulse = neonPulseAmount +
                (1f - neonPulseAmount) *
                ((Mathf.Sin(t * s.pulseSpeed + s.pulseOffset) + 1f) * 0.5f);

            Color emissive = s.baseColor * pulse * 2.2f;

            s.rend.GetPropertyBlock(s.block);
            s.block.SetColor("_EmissionColor", emissive);
            s.rend.SetPropertyBlock(s.block);
        }
    }

    private void ShimmerMirror()
    {
        if (mirrorRenderer == null) return;
        mirrorTime += Time.deltaTime * mirrorShimmerSpeed;

        float shimmer = (Mathf.Sin(mirrorTime) + 1f) * 0.5f;
        Color c = Color.Lerp(
            mirrorColor,
            mirrorColor + new Color(0.1f, 0.05f, 0.2f),
            shimmer);

        mirrorRenderer.GetPropertyBlock(mirrorBlock);
        mirrorBlock.SetColor("_BaseColor", c);
        mirrorRenderer.SetPropertyBlock(mirrorBlock);
    }

    // ??? HELPERS ?????????????????????????????????????????????????????
    private GameObject CreatePrimitive(
        PrimitiveType type, string name,
        Vector3 position, Vector3 scale, Color color)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(transform);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        Destroy(obj.GetComponent<Collider>());

        var rend = obj.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        rend.material = mat;

        return obj;
    }

    private void SetMaterialProps(
        Renderer rend, Color color,
        float smoothness = 0.5f, float metallic = 0f)
    {
        var mat = rend.material;
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
    }

    private void OnDestroy()
    {
        RenderSettings.fog = false;
    }
}