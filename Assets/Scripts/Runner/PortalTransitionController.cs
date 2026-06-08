using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalTransitionController : MonoBehaviour
{
    public static PortalTransitionController Instance;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;

    [Header("Settings")]
    public bool useFade = true;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }


    private Coroutine activeTransition;

    public void StartTransition(ProfessionType type)
    {
        RunContext.Instance.StartRun(type);
        Debug.Log($"[PortalTransition] StartTransition = {type}");
        if (isTransitioning)
            return;

        if (activeTransition != null)
            StopCoroutine(activeTransition);

        activeTransition = StartCoroutine(TransitionSequence(type));
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ResetTransitionFlag;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ResetTransitionFlag;
    }

    private void ResetTransitionFlag(Scene scene, LoadSceneMode mode)
    {
         Debug.Log($"SCENE LOADED {scene.name}");
        isTransitioning = false;
        if (activeTransition != null)
        {
            Debug.Log("STOPPING TRANSITION");
            StopCoroutine(activeTransition);
            activeTransition = null;
        }

    }

    private IEnumerator TransitionSequence(ProfessionType type)
    {
        isTransitioning = true;
        ChallengeManager.Instance?.StartChallenge(type);

        RunnerController.Instance?.EnterChallenge();
        var player = RunnerController.Instance != null ? RunnerController.Instance.player : null;
        if (player != null)
            player.EnableInput(false);

        if (useFade)
            yield return StartCoroutine(FadeIn());

        string sceneName = "Challenge_" + type;
        yield return SceneManager.LoadSceneAsync(sceneName);

        Debug.Log("[PortalTransition] BEFORE StartChallenge");

        ChallengeManager.Instance?.StartChallenge(type);

        Debug.Log("[PortalTransition] AFTER StartChallenge");

        while (!ChallengeManager.Instance.IsChallengeFinished)
            yield return null;

        var data = ChallengeManager.Instance.GetProfessionData(type);
        if (CourseScreenController.Instance != null && data != null)
            CourseScreenController.Instance.ShowResult(data, type, ChallengeManager.Instance.LastSuccess);

        while (CourseScreenController.Instance != null && !CourseScreenController.Instance.IsClosed)
            yield return null;

        if (useFade)
            yield return StartCoroutine(FadeOut());

        yield return SceneManager.LoadSceneAsync("Game");

        RunnerController.Instance?.RestartRun();
        if (player != null)
            player.EnableInput(true);

        isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvas == null)
            yield break;

        fadeCanvas.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (fadeCanvas == null)
            yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
    }
}
