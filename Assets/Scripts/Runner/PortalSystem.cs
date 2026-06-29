using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PortalSystem : MonoBehaviour
{
    public static PortalSystem Instance;

    [Header("Portal Prefab")]
    public GameObject portalPrefab;

    [Header("Settings")]
    public float portalDistanceAhead = 3f;
    public float portalScaleTime = 0.3f;      // было 0.4 — слишком быстро
    public float suckDuration = 1.2f;          // было 0.6
    public float suckStrength = 6f;            // было 12 — слишком резко
    public float offsetPlayer = 1f;

    [Header("Portal Grow")]
    public float portalGrowDuration = 1f;    // длительность роста на камеру
    public float portalFinalScale = 20f;       // размер перекрытия экрана
    private bool sceneReady = false;
    [Header("References")]
    public Transform player;
    public PlayerController playerController;

    private GameObject activePortal;
    private bool isLoadingScene = false;       // флаг чтобы не дёргать дважды

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingScene = false;
        StartCoroutine(FindPlayerReferences());
    }

    public void ShowPortal(ProfessionType type)
    {
        Debug.Log($"[PortalSystem] ShowPortal type = {type}");
        if (portalPrefab == null || player == null)
        {
            Debug.LogWarning("PortalSystem: Missing references.");
            return;
        }

        if (activePortal != null)
            Destroy(activePortal);

        // Позиция перед игроком + выше
        Vector3 pos = player.position + player.forward * portalDistanceAhead + Vector3.up * offsetPlayer;
        activePortal = Instantiate(portalPrefab, pos, Quaternion.identity);

        // Поворачиваем портал лицом к игроку
        activePortal.transform.LookAt(player.position);

        Color color = GetProfessionColor(type);
        var renderer = activePortal.GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = color;

        StartCoroutine(PortalSequence(type));
    }

    private IEnumerator PortalSequence(ProfessionType type)
    {
        // 1. Плавное появление портала
        yield return StartCoroutine(ScaleIn(activePortal));

        // 2. Небольшая пауза
        yield return new WaitForSecondsRealtime(0.3f);

        // 3. Плавное засасывание игрока
        yield return StartCoroutine(SuckPlayerIntoPortal(activePortal.transform));

        // 4. Скрываем игрока
        if (playerController != null)
            playerController.gameObject.SetActive(false);

        // 5. Запускаем фейд ПАРАЛЛЕЛЬНО с ростом портала
        //    Фейд начнёт затемняться пока портал летит на камеру
        if (PortalTransitionController.Instance != null)
            PortalTransitionController.Instance.StartFadeIn();

        // 6. Портал МЕДЛЕННО растёт и летит на камеру
        yield return StartCoroutine(GrowPortalToCamera(activePortal));

        // 7. Портал закрыл камеру — фейд уже чёрный (или почти)
        //    Запускаем загрузку сцены
        PortalTransitionController.Instance?.StartSceneLoad(type);

        // 8. Ждём пока фейд точно перекроет всё
        yield return new WaitForSecondsRealtime(0.3f);

        // 9. Убираем портал
        Destroy(activePortal);
        activePortal = null;
    }
    // Этот метод вызовет PortalTransitionController когда сцена загрузится
    public void OnSceneReady()
    {
        sceneReady = true;
    }
    public void MarkSceneLoading(bool loading)
    {
        isLoadingScene = loading;
    }

    private IEnumerator ScaleIn(GameObject portal)
    {
        if (portal == null) yield break;

        portal.transform.localScale = Vector3.zero;

        float t = 0f;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one;

        while (t < portalScaleTime)
        {
            t += Time.unscaledDeltaTime;
            float k = t / portalScaleTime;

            // Ease-out: быстро в начале, плавно в конце
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            portal.transform.localScale = Vector3.Lerp(start, end, eased);

            yield return null;
        }

        portal.transform.localScale = end;
    }

    private IEnumerator SuckPlayerIntoPortal(Transform portal)
    {
        if (player == null || portal == null) yield break;

        if (playerController != null)
            playerController.EnableInput(false);

        float t = 0f;
        Vector3 startPos = player.position;

        // Точка назначения — центр портала
        Vector3 targetPos = portal.position;

        while (t < suckDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / suckDuration;

            // Ease-in: медленно в начале, быстро в конце (засасывает)
            float eased = k * k;

            player.position = Vector3.Lerp(startPos, targetPos, eased);

            // Лёгкое уменьшение игрока при приближении (эффект перспективы)
            float scale = Mathf.Lerp(1f, 0.1f, eased);
            player.localScale = Vector3.one * scale;

            yield return null;
        }

        player.position = targetPos;
        player.localScale = Vector3.one * 0.1f; // почти исчез
    }

    private IEnumerator GrowPortalToCamera(GameObject portal)
    {
        if (portal == null) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 startPos = portal.transform.position;
        Quaternion startRot = portal.transform.rotation;
        Vector3 startScale = portal.transform.localScale;

        // Портал летит ПРЯМО В ЦЕНТР КАМЕРЫ
        Vector3 targetPos = cam.transform.position + cam.transform.forward * 0.5f;
        Quaternion targetRot = Quaternion.LookRotation(cam.transform.forward);
        Vector3 endScale = Vector3.one * portalFinalScale;

        float t = 0f;
        while (t < portalGrowDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / portalGrowDuration;

            // ease-in-out: медленно в начале, ускоряется в середине, медленно в конце
            float eased = k < 0.5f
                ? 4f * k * k * k
                : 1f - Mathf.Pow(-2f * k + 2f, 3f) / 2f;

            portal.transform.position = Vector3.Lerp(startPos, targetPos, eased);
            portal.transform.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            portal.transform.localScale = Vector3.Lerp(startScale, endScale, eased);

            yield return null;
        }

        // Финал — портал в центре экрана
        portal.transform.position = targetPos;
        portal.transform.rotation = targetRot;
        portal.transform.localScale = endScale;
    }

    private IEnumerator FindPlayerReferences()
    {
        yield return null;
        yield return null;

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (portalPrefab == null)
            Debug.LogWarning("PortalSystem: portalPrefab is not assigned!");

        if (player == null)
            Debug.LogWarning("PortalSystem: player not found in scene!");
    }

    private Color GetProfessionColor(ProfessionType type)
    {
        return type switch
        {
            ProfessionType.IT => new Color32(0x00, 0x7A, 0xFF, 255),
            ProfessionType.Design => new Color32(0xAF, 0x52, 0xDE, 255),
            ProfessionType.Marketing => new Color32(0xFF, 0x95, 0x00, 255),
            ProfessionType.Analytics => new Color32(0x34, 0xC7, 0x59, 255),
            ProfessionType.Media => new Color32(0xFF, 0x2D, 0x55, 255),
            ProfessionType.Engineering => new Color32(0xFF, 0xCC, 0x00, 255),
            ProfessionType.Management => new Color32(0xFF, 0x3B, 0x30, 255),
            _ => Color.white
        };
    }
}