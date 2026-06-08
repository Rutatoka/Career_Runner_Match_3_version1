using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultWindow : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject resultPanel; // Панель с результатом
    public TMP_Text titleText;
    public TMP_Text messageText;
    public TMP_Text scoreText;
    public Button replayButton;
    public Button menuButton;

    [Header("Settings")]
    public string menuSceneName = "MainMenu";
    public string replaySceneName = ""; // Если пусто - перезагружаем текущую

    [Header("Animation")]
    public float showDelay = 0.5f;
    public float animationDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
            canvasGroup = resultPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        }

        // Подписываемся на события
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayClicked);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    public void ShowSuccess(int correctBlocks, int totalBlocks, float timeLeft)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = "Отлично!";
            titleText.color = Color.green;
        }

        if (messageText != null)
        {
            messageText.text = "Вы успешно прошли испытание!";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Правильно: {correctBlocks}/{totalBlocks}\n" +
                           $"Оставшееся время: {Mathf.CeilToInt(timeLeft)}с";
        }

        if (replayButton != null)
            replayButton.gameObject.SetActive(true);
        if (menuButton != null)
            menuButton.gameObject.SetActive(true);
   
        StartCoroutine(ShowAnimation());
    }

    public void ShowFailure(int correctBlocks, int totalBlocks, string reason)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = "Не получилось";
            titleText.color = Color.red;
        }

        if (messageText != null)
        {
            string failReason = reason switch
            {
                "time" => "Время вышло! Попробуйте ещё раз.",
                "attempts" => "Слишком много ошибок! Попробуйте снова.",
                "wrong" => "Не все было правильно.",
                _ => "Попробуйте ещё раз!"
            };
            messageText.text = failReason;
        }

        if (scoreText != null)
        {
            scoreText.text = $"Правильно: {correctBlocks}/{totalBlocks}\n" +
                           "Попробуйте улучшить результат!";
        }

        if (replayButton != null)
            replayButton.gameObject.SetActive(true);
        if (menuButton != null)
            menuButton.gameObject.SetActive(true);
     
        StartCoroutine(ShowAnimation());
    }

    private System.Collections.IEnumerator ShowAnimation()
    {
        // Начальное состояние
        canvasGroup.alpha = 0f;
        resultPanel.transform.localScale = Vector3.one * 0.8f;

        // Задержка перед показом
        yield return new WaitForSeconds(showDelay);

        // Анимация появления
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Используем плавную кривую
            t = 1 - Mathf.Pow(1 - t, 3); // Ease out

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            resultPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        resultPanel.transform.localScale = Vector3.one;
    }

    private void OnReplayClicked()
    {
        PreferenceAnalyzer.Instance?.RegisterRunCompleted();
        GameManager.Instance?.StartGame();
    }


    private void OnMenuClicked()
    {
        PreferenceAnalyzer.Instance?.RegisterRunCompleted();
        Debug.Log($"[ResultWindow] Menu clicked, loading: {menuSceneName}");
       GameManager.Instance?.GoToMenu();
    }
}