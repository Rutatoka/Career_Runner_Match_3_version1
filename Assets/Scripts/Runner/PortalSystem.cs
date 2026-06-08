using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class PortalSystem : MonoBehaviour
{
    public static PortalSystem Instance;

    [Header("Portal Prefab")]
    public GameObject portalPrefab;

    [Header("Settings")]
    public float portalDistanceAhead = 6f;
    public float portalScaleTime = 0.4f;
    public float suckDuration = 0.6f;
    public float suckStrength = 12f;

    [Header("References")]
    public Transform player;
    public PlayerController playerController;

    private GameObject activePortal;

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

        Vector3 pos = player.position + player.forward * portalDistanceAhead;
        activePortal = Instantiate(portalPrefab, pos, Quaternion.identity);

        Color color = GetProfessionColor(type);
        var renderer = activePortal.GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = color;

        StartCoroutine(PortalSequence(type));
    }

    private IEnumerator PortalSequence(ProfessionType type)
    {
        yield return StartCoroutine(ScaleIn(activePortal));
        yield return StartCoroutine(SuckPlayerIntoPortal(activePortal.transform));

        PortalTransitionController.Instance?.StartTransition(type);

        Destroy(activePortal);
        activePortal = null;
    }

    private IEnumerator ScaleIn(GameObject portal)
    {
        if (portal == null) yield break;

        float t = 0f;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one;

        portal.transform.localScale = start;

        while (t < portalScaleTime)
        {
            t += Time.unscaledDeltaTime;
            float k = t / portalScaleTime;
            portal.transform.localScale = Vector3.Lerp(start, end, k);
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

        while (t < suckDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / suckDuration;

            player.position = Vector3.Lerp(startPos, portal.position, k);
            player.position += (portal.position - player.position).normalized * suckStrength * Time.unscaledDeltaTime;

            yield return null;
        }

        player.position = portal.position;
    }
  

    private IEnumerator FindPlayerReferences()
    {
        // ждём пока сцена полностью загрузится
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

        if (playerController == null)
            Debug.LogWarning("PortalSystem: playerController not found!");
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
