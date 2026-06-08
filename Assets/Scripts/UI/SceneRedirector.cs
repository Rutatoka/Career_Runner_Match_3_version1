using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChallengeRedirector : MonoBehaviour
{
    public static ChallengeRedirector Instance;

    [Header("Button Search")]
    public string[] possibleButtonNames = { "BackButton", "ExitButton", "ReturnButton" };
    public string buttonTag = "BackToMenuBtn"; // если хочешь искать по тегу

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Проверяем, является ли сцена челенджем
        if (scene.name.StartsWith("Challenge_"))
        {
            StartCoroutine(AssignButtonDelayed());
        }
    }

    private IEnumerator AssignButtonDelayed()
    {
        // ждём 1 кадр, чтобы UI успел появиться
        yield return null;

        Button btn = FindButton();
        if (btn == null)
        {
            Debug.LogWarning("ChallengeRedirector: кнопка не найдена.");
            yield break;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            ChallengeManager.Instance?.AbortChallenge();
            PreferenceAnalyzer.Instance?.RegisterRunCompleted();
            GameManager.Instance.GoToMenu();
        });

       // Debug.Log("ChallengeRedirector: кнопка переназначена → GameManager.GoToMenu()");
    }

    private Button FindButton()
    {
        // 1) По имени
        foreach (string name in possibleButtonNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Button b = obj.GetComponent<Button>();
                if (b != null) return b;
            }
        }

        // 2) По тегу
        GameObject tagged = GameObject.FindWithTag(buttonTag);
        if (tagged != null)
        {
            Button b = tagged.GetComponent<Button>();
            if (b != null) return b;
        }

        // 3) Любая кнопка на сцене (fallback)
        return FindObjectOfType<Button>();
    }
}
