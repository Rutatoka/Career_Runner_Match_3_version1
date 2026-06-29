using UnityEngine;

public class MountainBackground : MonoBehaviour
{
    [Header("Префаб горы")]
    public GameObject mountainPrefab;

    [Header("Позиционирование")]
    public float distanceFromCamera = 500f;
    public float yOffset = -50f;
    public float mountainScale = 200f;

    [Header("Параллакс (опционально)")]
    public bool useParallax = true;
    [Range(0f, 1f)]
    public float parallaxStrength = 0.05f;

    [Header("Визуальные эффекты")]
    public Color mountainColor = new Color(0.15f, 0.08f, 0.2f, 0.9f);

    [Header("Ссылки")]
    public Transform playerTransform;

    private GameObject mountainInstance;
    private Transform cameraTransform;
    private Renderer[] mountainRenderers;
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        cameraTransform = Camera.main?.transform;
        if (cameraTransform == null)
        {
            var cam = FindObjectOfType<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        if (playerTransform == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerTransform = player.transform;
        }
    }

    private void Start()
    {
        if (mountainPrefab == null)
        {
            Debug.LogError("MountainBackground: Не назначен префаб горы!");
            return;
        }

        SpawnMountain();

        if (cameraTransform != null)
            lastCameraPosition = cameraTransform.position;
    }

    private void SpawnMountain()
    {
        // Создаем гору ДОЧЕРНИМ объектом камеры
        mountainInstance = Instantiate(mountainPrefab, cameraTransform);
        mountainInstance.transform.localPosition = new Vector3(0, yOffset, distanceFromCamera);
        mountainInstance.transform.localRotation = Quaternion.identity;
        mountainInstance.name = "MountainBackground";
        mountainInstance.transform.localScale = Vector3.one * mountainScale;

        // Получаем все рендереры
        mountainRenderers = mountainInstance.GetComponentsInChildren<Renderer>();

        // Применяем цвет и отключаем туман в материале
        FixMaterials();
    }

    private void FixMaterials()
    {
        if (mountainRenderers == null) return;

        foreach (Renderer rend in mountainRenderers)
        {
            // Создаем копию материала чтобы не менять оригинал
            Material mat = new Material(rend.material);

            // Меняем шейдер на Unlit (без освещения и тумана)
            mat.shader = Shader.Find("Universal Render Pipeline/Unlit");

            // Если URP Unlit не нашелся, используем обычный Unlit
            if (mat.shader == null)
                mat.shader = Shader.Find("Unlit/Color");

            // Если и такого нет, используем Standard и отключаем туман
            if (mat.shader == null)
            {
                mat.shader = Shader.Find("Standard");
                mat.DisableKeyword("_FOG");
                mat.SetFloat("_Fog", 0f);
            }

            // Устанавливаем цвет
            mat.color = mountainColor;

            // Применяем материал
            rend.material = mat;
        }
    }

    private void LateUpdate()
    {
        if (mountainInstance == null || cameraTransform == null) return;
        UpdateMountainPosition();
    }

    private void UpdateMountainPosition()
    {
        if (cameraTransform == null) return;

        Vector3 localPosition;

        if (useParallax && playerTransform != null)
        {
            Vector3 cameraPos = cameraTransform.position;
            float parallaxZ = cameraPos.z + (playerTransform.position.z - cameraPos.z) * parallaxStrength;
            localPosition = new Vector3(0, yOffset, parallaxZ + distanceFromCamera);
        }
        else
        {
            localPosition = new Vector3(0, yOffset, distanceFromCamera);
        }

        mountainInstance.transform.localPosition = localPosition;
    }

    private void ApplyColor()
    {
        if (mountainRenderers == null) return;

        foreach (Renderer rend in mountainRenderers)
        {
            rend.material.color = mountainColor;
        }
    }

    public void SetMountainColor(Color newColor)
    {
        mountainColor = newColor;
        ApplyColor();
    }

    private void OnDestroy()
    {
        if (mountainInstance != null)
            Destroy(mountainInstance);
    }
}