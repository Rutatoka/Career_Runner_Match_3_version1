using UnityEngine;
using System;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager Instance;

    [Header("Settings")]
    public int requiredCorrect = 3;
    public int totalActions = 5;
    private bool alreadyFinished;
    [Header("Profession Mapping")]
    public ProfessionObjectData[] professionObjects;

    [Header("Debug")]
    public bool autoComplete = false;

    private int correctActions = 0;
    private int actionsDone = 0;
    public bool IsChallengeFinished { get; private set; } = false;
    public bool LastSuccess { get; private set; } = false;

    public event Action OnChallengeStarted;
    public event Action<bool> OnChallengeFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ChallengeManager] Duplicate instance, destroying");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[ChallengeManager] Awake, init DB");
        ProfessionSystemDatabase.Init(professionObjects);
    }
    private void Start()
    {
        alreadyFinished = false;
    }
    public void StartChallenge(ProfessionType type)
    {
        Debug.Log($"[ChallengeManager] StartChallenge SET = {type} hash={GetHashCode()}");
        alreadyFinished = false;
        IsChallengeFinished = false;
        LastSuccess = false;
        RunContext.Instance.StartRun(type);
        correctActions = 0;
        actionsDone = 0;
        IsChallengeFinished = false;

      //  Debug.Log($"[ChallengeManager] StartChallenge for {type}");
        OnChallengeStarted?.Invoke();
    }

    public void FinishChallenge(bool success)
    {
        Debug.Log(
    $"FinishChallenge ENTER success={success} alreadyFinished={alreadyFinished}"
);
        if (alreadyFinished) return;
        alreadyFinished = true;
        if (RunContext.Instance == null)
        {
            Debug.LogError("RunContext is missing!");
            return;
        } 
        IsChallengeFinished = true;
        LastSuccess = success;
        var type = RunContext.Instance.CurrentProfession;
        Debug.Log($"Current profession = {RunContext.Instance.CurrentProfession}");
        if (success)
            PreferenceAnalyzer.Instance?.RegisterChallengeSuccess(type);
        else
            PreferenceAnalyzer.Instance?.RegisterChallengeFail(type);

        // 👇 ВАЖНО: run всегда закрывается
        PreferenceAnalyzer.Instance?.RegisterRunCompleted();

        OnChallengeFinished?.Invoke(success);
    }
    public void FailChallenge(string reason)
    {
        if (IsChallengeFinished)
        {
            Debug.Log("[ChallengeManager] FailChallenge called but challenge already finished");
            return;
        }
        PreferenceAnalyzer.Instance?.RegisterChallengeFail(RunContext.Instance.CurrentProfession);
        LastSuccess = false;
        IsChallengeFinished = true;

       Debug.Log($"[ChallengeManager] Challenge FAILED for {RunContext.Instance.CurrentProfession}, reason={reason}");

        // Если хочешь учитывать провалы в анализе предпочтений:
        // PreferenceAnalyzer.Instance?.RegisterChallengeFail(currentType);

        OnChallengeFinished?.Invoke(false);
    }
    public void RegisterAction(bool isCorrect)
    {
        if (IsChallengeFinished)
        {
            Debug.Log("[ChallengeManager] RegisterAction called but challenge already finished");
            return;
        }

        actionsDone++;

        if (isCorrect)
            correctActions++;

        Debug.Log($"[ChallengeManager] RegisterAction: isCorrect={isCorrect}, " +
                  $"actionsDone={actionsDone}/{totalActions}, correct={correctActions}/{requiredCorrect}");

        if (autoComplete)
        {
            Debug.Log("[ChallengeManager] autoComplete ON → force success");
            FinishChallenge(true);
            return;
        }

        if (actionsDone >= totalActions)
        {
            bool success = correctActions >= requiredCorrect;
            Debug.Log($"[ChallengeManager] Actions limit reached → success={success}");
            FinishChallenge(success);
        }
    }
    public void AbortChallenge()
    {
        if (IsChallengeFinished)
            return;

        IsChallengeFinished = true;
        LastSuccess = false;
      //  Debug.Log("ABORT CALLED");
       // Debug.Log("[ChallengeManager] Challenge ABORTED");

        // ВАЖНО: решаешь, считать ли это фейлом или просто выходом
     //   PreferenceAnalyzer.Instance?.RegisterRunCompleted();

        OnChallengeFinished?.Invoke(false);
    }
    public ProfessionData GetProfessionData(ProfessionType type)
    {
        if (professionObjects == null)
        {
            Debug.LogWarning("[ChallengeManager] professionObjects is NULL");
            return null;
        }

        foreach (var obj in professionObjects)
        {
            if (obj == null) continue;
            if (obj.professionType != type) continue;
            if (obj.professionData == null) continue;
            return obj.professionData;
        }

        Debug.LogWarning($"[ChallengeManager] No ProfessionData for {type}");
        return null;
    }
}
