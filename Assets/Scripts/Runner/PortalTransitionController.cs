using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class PortalTransitionController : MonoBehaviour
{
    public static PortalTransitionController Instance;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public bool useFade = true;
    public Color fadeColor = Color.black;

    private CanvasGroup fadeCanvasGroup;
    private GameObject fadeObject;
    private bool isTransitioning = false;
    private Coroutine activeTransition;

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
        Debug.Log($"[PortalTransition] OnSceneLoaded scene={scene.name} isTransitioning={isTransitioning}");

        CreateFadeCanvas();

        if (!isTransitioning)
        {
            if (fadeCanvasGroup != null)
            {
                Debug.Log("[PortalTransition] Setting alpha=0 (not transitioning)");
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            Debug.Log($"[PortalTransition] Transitioning — starting FadeOut from OnSceneLoaded");
            // Корутина TransitionSequence уже убита ResetFlag, поэтому запускаем FadeOut отсюда
            StartCoroutine(FadeOutAfterLoad());
        }
    }

    private IEnumerator FadeOutAfterLoad()
    {
        yield return null;

        Debug.Log($"[FadeOutAfterLoad] START alpha={fadeCanvasGroup?.alpha}");
        yield return StartCoroutine(FadeOutRoutine());
        Debug.Log("[FadeOutAfterLoad] DONE");

        // Запускаем челлендж после появления сцены
        var type = RunContext.Instance?.CurrentProfession ?? ProfessionType.IT;
        ChallengeManager.Instance?.StartChallenge(type);
    }

    private void CreateFadeCanvas()
    {
        Debug.Log("[PortalTransition] CreateFadeCanvas called");
        if (fadeObject != null)
        {
            Destroy(fadeObject);
            fadeObject = null;
            fadeCanvasGroup = null;
        }

        fadeObject = new GameObject("FadeCanvas");
        fadeObject.tag = "FadeCanvas";

        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeObject.transform, false);

        Image image = imageObj.AddComponent<Image>();
        image.color = fadeColor;
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.sizeDelta = Vector2.zero;
        image.raycastTarget = true;

        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 1f;        // ← БЫЛО 0f, СТАЛО 1f
        fadeCanvasGroup.blocksRaycasts = true;  // ← БЫЛО false, СТАЛО true
        Debug.Log($"[PortalTransition] Created: {fadeObject.name}, alpha={fadeCanvasGroup.alpha}");
    }

    // Вызывается из PortalSystem — запускает затемнение
    public void StartFadeIn()
    {
        if (!useFade || isTransitioning) return;
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        GameObject obj = GameObject.FindWithTag("FadeCanvas");
        if (obj != null)
        {
            fadeCanvasGroup = obj.GetComponent<CanvasGroup>();
            fadeObject = obj;
        }

        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    // Загрузка сцены
    public void StartSceneLoad(ProfessionType type)
    {
        if (isTransitioning) return;
        if (activeTransition != null)
            StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(TransitionSequence(type));
    }

    private IEnumerator TransitionSequence(ProfessionType type)
    {
        isTransitioning = true;
        RunContext.Instance.StartRun(type);

        RunnerController.Instance?.EnterChallenge();
        var runnerPlayer = RunnerController.Instance != null ? RunnerController.Instance.player : null;
        if (runnerPlayer != null)
            runnerPlayer.EnableInput(false);

        string sceneName = "Challenge_" + type;
        yield return SceneManager.LoadSceneAsync(sceneName);

        // OnSceneLoaded уже создал fadeCanvas с альфой 1 (CreateFadeCanvas по умолчанию)
        // Просто делаем FadeOut
        yield return StartCoroutine(FadeOutRoutine());

        ChallengeManager.Instance?.StartChallenge(type);

        while (ChallengeManager.Instance != null && !ChallengeManager.Instance.IsChallengeFinished)
            yield return null;

        if (ChallengeManager.Instance != null)
        {
            var data = ChallengeManager.Instance.GetProfessionData(type);
            if (CourseScreenController.Instance != null && data != null)
                CourseScreenController.Instance.ShowResult(data, type, ChallengeManager.Instance.LastSuccess);
        }

        while (CourseScreenController.Instance != null && !CourseScreenController.Instance.IsClosed)
            yield return null;

        // Затемняем перед возвратом
        yield return StartCoroutine(FadeInRoutine());

        // Возврат в игру
        yield return SceneManager.LoadSceneAsync("Game");



        yield return StartCoroutine(FadeOutRoutine());

        RunnerController.Instance?.RestartRun();
        if (runnerPlayer != null)
            runnerPlayer.EnableInput(true);

        isTransitioning = false;
    }

    private IEnumerator FadeOutRoutine()
    {
        // Всегда ищем заново — надёжнее
        GameObject obj = GameObject.FindWithTag("FadeCanvas");
        if (obj != null)
        {
            fadeCanvasGroup = obj.GetComponent<CanvasGroup>();
            fadeObject = obj;
        }

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[FadeOutRoutine] fadeCanvasGroup is NULL!");
            yield break;
        }

        Debug.Log($"[FadeOutRoutine] START alpha={fadeCanvasGroup.alpha}");

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        Debug.Log("[FadeOutRoutine] DONE alpha=0");
    }

    private void OnEnable() => SceneManager.sceneLoaded += ResetFlag;
    private void OnDisable() => SceneManager.sceneLoaded -= ResetFlag;

    private void ResetFlag(Scene scene, LoadSceneMode mode)
    {
        isTransitioning = false;
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
            activeTransition = null;
        }
    }
}