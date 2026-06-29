using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDecorator : MonoBehaviour
{
    [Header("Перспективный масштаб")]
    public Transform playerTransform;
    public float scaleAtPlayer = 8f;
    public float scaleAtDistance = 31f;
    public float maxDistance = 60f;

    [Header("Префаб столба")]
    public GameObject pillarPrefab;

    [Header("Расположение")]
    public float sideOffset = 7f;
    public float pillarSpacing = 5f;
    public float pillarHeight = 3f;
    public int pillarsPerSide = 3;

    [Header("Анимация")]
    public float swaySpeed = 0.8f;
    public float swayAmount = 15f;
    public float pulseSpeed = 0.5f;
    public float alphaMin = 0.2f;
    public float alphaMax = 0.55f;

    [Header("Переход цвета")]


    private readonly List<PillarData> activePillars = new List<PillarData>();
    private Color currentDisplayColor;
    private Color targetColor;


    private ProfessionType lastCollectedType = ProfessionType.None;
    private bool hasCollectedType = false;
    private static readonly Color ColorDefault = new Color(0.7f, 0.5f, 1f, 0.35f);

    private class PillarData
    {
        public Transform root;
        public Vector3 basePosition;
        public float swayOffset;
        public Renderer rend;
        public MaterialPropertyBlock block;
    }

    private void Awake()
    {
        currentDisplayColor = ColorDefault;
        targetColor = ColorDefault;
    }

    private void Update()
    {
        UpdateTargetColor();
        currentDisplayColor = targetColor;
        currentDisplayColor.a = Mathf.Lerp(
            currentDisplayColor.a,
            targetColor.a,
            Time.deltaTime * 8f
        );
        AnimatePillars();
    }

    public void DecorateTile(GameObject tile, float tileWorldZ)
    {
        if (pillarPrefab == null || tile == null) return;

        for (int i = 0; i < pillarsPerSide; i++)
        {
            float zOffset = i * pillarSpacing + pillarSpacing * 0.5f;
            SpawnPillar(tile, tileWorldZ + zOffset, -sideOffset);
            SpawnPillar(tile, tileWorldZ + zOffset, sideOffset);
        }
    }

    private void SpawnPillar(GameObject tile, float worldZ, float xOffset)
    {
        Vector3 worldPos = new Vector3(xOffset, pillarHeight, worldZ);

        GameObject obj = SimplePool.Instance != null
            ? SimplePool.Instance.Spawn(pillarPrefab, worldPos, Quaternion.identity, tile.transform)
            : Instantiate(pillarPrefab, worldPos, Quaternion.identity, tile.transform);

        if (obj == null) return;

        // Случайный поворот вокруг Y — каждый столб чуть другой
        obj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        var rend = obj.GetComponent<Renderer>();
        if (rend == null) rend = obj.GetComponentInChildren<Renderer>();

        var data = new PillarData
        {
            root = obj.transform,
            basePosition = worldPos,
            swayOffset = Random.Range(0f, Mathf.PI * 2f),
            rend = rend,
            block = new MaterialPropertyBlock()
        };

        activePillars.Add(data);
        ApplyColorToBlock(data, currentDisplayColor);
    }

    private void AnimatePillars()
    {
        float time = Time.time;

        if (playerTransform == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }

        for (int i = activePillars.Count - 1; i >= 0; i--)
        {
            var data = activePillars[i];

            if (data.root == null)
            {
                activePillars.RemoveAt(i);
                continue;
            }

            if (playerTransform != null)
            {
                float dist = data.root.position.z - playerTransform.position.z;

                // Столбы ВПЕРЕДИ игрока: dist > 0 — высокие вдали, низкие рядом
                // Столбы ПОЗАДИ: dist < 0 — просто маленькие, скоро уйдут в пул
                float scaleY;
                if (dist >= 0f)
                {
                    // Впереди: от маленького у игрока до большого вдали
                    float t = Mathf.Clamp01(dist / maxDistance);
                    scaleY = Mathf.Lerp(scaleAtPlayer, scaleAtDistance, t);
                }
                else
                {
                    // Позади: фиксируем минимальный масштаб
                    scaleY = scaleAtPlayer;
                }

                data.root.localScale = new Vector3(
                    data.root.localScale.x,
                    scaleY,
                    data.root.localScale.z
                );
            }

            // Покачивание
            float sway = Mathf.Sin(time * swaySpeed + data.swayOffset) * swayAmount;
            data.root.rotation = Quaternion.Euler(sway, data.root.eulerAngles.y, 0f);

            // Пульсация прозрачности — позади почти невидимые
            float distForAlpha = data.root.position.z - playerTransform.position.z;
            float alphaMult = distForAlpha >= 0f ? 1f : 0.1f;

            float alpha = Mathf.Lerp(
                alphaMin,
                alphaMax,
                (Mathf.Sin(time * pulseSpeed + data.swayOffset) + 1f) * 0.5f
            ) * alphaMult;

            Color c = currentDisplayColor;
            c.a = alpha;
            ApplyColorToBlock(data, c);
        }
    }

    private void ApplyColorToBlock(PillarData data, Color color)
    {
        if (data.rend == null) return;
        data.rend.GetPropertyBlock(data.block);
        data.block.SetColor("_BaseColor", color);
        data.rend.SetPropertyBlock(data.block);
    }


    private void UpdateTargetColor()
    {
        if (ProfessionSystem.Instance == null)
        {
            targetColor = ColorDefault;
            return;
        }

        // Берём текущий доминантный тип
        ProfessionType currentDominant = ProfessionSystem.Instance.GetDominantType();

        // Если что-то собрано (не None) — запоминаем
        if (currentDominant != ProfessionType.None)
        {
            lastCollectedType = currentDominant;
            hasCollectedType = true;
        }

        // Используем последний собранный тип, а не текущий доминантный
        ProfessionType typeToUse = hasCollectedType ? lastCollectedType : ProfessionType.None;

        if (typeToUse == ProfessionType.None)
        {
            targetColor = ColorDefault;
            return;
        }

        ProfessionObjectData data = GetProfessionObjectData(typeToUse);
        if (data != null)
        {
            targetColor = data.directionColor;
            targetColor.a = 0.4f;
        }
        else
        {
            targetColor = ColorDefault;
        }
    }

    // Новый метод — ищет ProfessionObjectData по типу
    private ProfessionObjectData GetProfessionObjectData(ProfessionType type)
    {
        // Ищем в ChallengeManager (если он есть)
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

        // Запасной вариант — ищем все ProfessionObjectData в ресурсах
        var allData = Resources.LoadAll<ProfessionObjectData>("");
        foreach (var data in allData)
        {
            if (data.professionType == type)
                return data;
        }

        return null;
    }

    // Вызывается из TileSpawner при удалении тайла —
    // чистим мёртвые ссылки чтобы список не рос бесконечно
    public void CleanupTile(GameObject tile)
    {
        if (tile == null) return;
        activePillars.RemoveAll(d =>
            d.root == null || d.root.IsChildOf(tile.transform));
    }
}