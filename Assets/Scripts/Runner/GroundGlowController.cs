using System.Collections.Generic;
using UnityEngine;

public class GroundGlowController : MonoBehaviour
{
    [Header("Ќастройки свечени€")]
    public float glowIntensity = 0.4f;
    public float pulseSpeed = 0.6f;
    public float pulseAmount = 0.15f;
    public float colorLerpSpeed = 3f;

    private static readonly Color[] ProfessionColors = new Color[]
    {
        new Color(0.00f, 0.48f, 1.00f),   // IT         Ч синий
        new Color(0.69f, 0.32f, 0.87f),   // Design     Ч фиолетовый
        new Color(1.00f, 0.58f, 0.00f),   // Marketing  Ч оранжевый
        new Color(0.20f, 0.78f, 0.35f),   // Analytics  Ч зелЄный
        new Color(1.00f, 0.18f, 0.33f),   // Media      Ч розовый
        new Color(1.00f, 0.80f, 0.00f),   // Engineering Ч жЄлтый
        new Color(1.00f, 0.23f, 0.19f),   // Management Ч красный
    };

    private static readonly Color ColorDefault = new Color(0.3f, 0.1f, 0.5f);

    private Color currentColor;
    private Color targetColor;

    // ¬се Road(3) которые сейчас живут на сцене
    private readonly List<GroundData> activeGrounds = new List<GroundData>();

    private class GroundData
    {
        public Renderer rend;
        public MaterialPropertyBlock block;
        public Transform root;
    }

    private void Awake()
    {
        currentColor = ColorDefault;
        targetColor = ColorDefault;
    }

    private void Update()
    {
        UpdateTargetColor();

        // ÷вет мен€ем мгновенно Ч без смешивани€ оттенков
        currentColor = targetColor;

        AnimateGrounds();
    }

    // ¬ызываетс€ из TileSpawner при спавне тайла
    public void RegisterTile(GameObject tile)
    {
        if (tile == null) return;

        // »щем Road(3) по имени среди дочерних объектов
        foreach (Transform child in tile.transform)
        {
            if (child.name.Contains("Road (3)") || child.name.Contains("Road(3)"))
            {
                RegisterGround(child);
                return;
            }

            // »щем глубже Ч Road(3) может быть внутри Road
            foreach (Transform grandChild in child)
            {
                if (grandChild.name.Contains("Road (3)") || grandChild.name.Contains("Road(3)"))
                {
                    RegisterGround(grandChild);
                    return;
                }
            }
        }
    }

    private void RegisterGround(Transform t)
    {
        var rend = t.GetComponent<Renderer>();
        if (rend == null) return;

        // ѕровер€ем что этот рендерер ещЄ не зарегистрирован
        foreach (var existing in activeGrounds)
        {
            if (existing.rend == rend) return;
        }

        var data = new GroundData
        {
            rend = rend,
            block = new MaterialPropertyBlock(),
            root = t
        };

        activeGrounds.Add(data);
    }

    private void AnimateGrounds()
    {
        float time = Time.time;

        for (int i = activeGrounds.Count - 1; i >= 0; i--)
        {
            var data = activeGrounds[i];

            if (data.rend == null || data.root == null)
            {
                activeGrounds.RemoveAt(i);
                continue;
            }

            // ѕульсаци€ интенсивности свечени€
            float pulse = glowIntensity +
                Mathf.Sin(time * pulseSpeed + i * 0.5f) * pulseAmount;

            Color emissionColor = currentColor * pulse;

            data.rend.GetPropertyBlock(data.block);
            // _EmissionColor Ч стандартное им€ в URP Lit
            data.block.SetColor("_EmissionColor", emissionColor);
            data.rend.SetPropertyBlock(data.block);
        }
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

    // ¬ызываетс€ из TileSpawner при удалении тайла
    public void CleanupTile(GameObject tile)
    {
        if (tile == null) return;
        activeGrounds.RemoveAll(d =>
            d.root == null || d.root.IsChildOf(tile.transform));
    }
}