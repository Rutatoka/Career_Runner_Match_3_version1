using UnityEngine;
using System;
using System.Collections.Generic;
[Serializable]
public class ProfessionStatsView
{
    public ProfessionType type;
    public float successRate;
    public int objects;
    public int portals;

    public int successes;
    public int failures;

    public float weight;

}
public class PreferenceAnalyzer : MonoBehaviour
{
    public static PreferenceAnalyzer Instance;

    [Header("Settings")]
    public int minRunsForRecommendation = 10;
    public int minSuccessChallenges = 1;
    public bool autoSave = true;

    [Header("Debug")]
    public ProfessionType dominantProfession = ProfessionType.None;

    private Dictionary<ProfessionType, int> objectsCollected = new();
    private Dictionary<ProfessionType, int> portalsActivated = new();
    private Dictionary<ProfessionType, int> challengesSuccess = new();
    private Dictionary<ProfessionType, int> challengesFailed = new();

    private int totalRuns = 0;

    public event Action<ProfessionType> OnDominantProfessionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitDictionaries();
        LoadStats();
    }
    public List<ProfessionStatsView> GetAllStats()
    {
        List<ProfessionStatsView> result = new();

        foreach (ProfessionType type in Enum.GetValues(typeof(ProfessionType)))
        {
            if (type == ProfessionType.None)
                continue;

            result.Add(GetStats(type));
        }

        return result;
    }
    public ProfessionStatsView GetStats(ProfessionType type)
    {
        int successes = GetSuccessChallenges(type);
        int failures = GetFailures(type);
        int totalAttempts = successes + failures;

        float successRate =
    totalAttempts > 0
    ? (float)successes / totalAttempts * 100f
    : 0f;

        return new ProfessionStatsView
        {
            type = type,
            objects = GetObjects(type),
            portals = GetPortals(type),
            successes = successes,
            failures = failures,
            successRate = successRate,
            weight = CalculateScore(type)
        };
    }
    private void InitDictionaries()
    {
        objectsCollected.Clear();
        portalsActivated.Clear();
        challengesSuccess.Clear();
        challengesFailed.Clear();

        foreach (ProfessionType type in Enum.GetValues(typeof(ProfessionType)))
        {
            if (type == ProfessionType.None)
                continue;

            objectsCollected.Add(type, 0);
            portalsActivated.Add(type, 0);
            challengesSuccess.Add(type, 0);
            challengesFailed.Add(type, 0);
        }
    }

    // =====================================================
    // REGISTRATION
    // =====================================================

    public void RegisterObjectPickup(ProfessionType type)
    {
        if (type == ProfessionType.None)
            return;

        objectsCollected[type]++;
        SaveStats();
    }

    public void RegisterPortal(ProfessionType type)
    {
        if (type == ProfessionType.None)
            return;

        portalsActivated[type]++;
        SaveStats();
    }

    public void RegisterChallengeSuccess(ProfessionType type)
    {
        Debug.Log($"[PreferenceAnalyzer] SUCCESS FOR {type}");
        if (type == ProfessionType.None)
            return;

        challengesSuccess[type]++;
        Debug.Log($"[PreferenceAnalyzer] {type} successes = {challengesSuccess[type]}");

        SaveStats();
    }
    public void RegisterChallengeFail(ProfessionType type)
    {
        if (type == ProfessionType.None)
            return;

        challengesFailed[type]++;

        SaveStats();

        Debug.Log(
            $"[PreferenceAnalyzer] FAIL {type} = {challengesFailed[type]}"
        );
    }
    public void RegisterRunCompleted()
    {
        totalRuns++;
       Debug.Log("RegisterRunCompleted CALLED");

        SaveStats();
    }

    // =====================================================
    // RECOMMENDATION
    // =====================================================

    public bool CanShowRecommendation()
    {
        return totalRuns >= minRunsForRecommendation
               && GetTotalSuccessChallenges() >= minSuccessChallenges;
    }

    public ProfessionType GetDominantProfession()
    {
        if (!CanShowRecommendation())
            return ProfessionType.None;

        float bestScore = -1;
        ProfessionType bestType = ProfessionType.None;

        foreach (ProfessionType type in objectsCollected.Keys)
        {
            float score = CalculateScore(type);

            if (score > bestScore)
            {
                bestScore = score;
                bestType = type;
            }
        }

        if (bestType != dominantProfession)
        {
            dominantProfession = bestType;
            OnDominantProfessionChanged?.Invoke(bestType);
        }

        return bestType;
    }

    public float CalculateScore(ProfessionType type)
    {
        return objectsCollected[type]
             + portalsActivated[type] * 2
             + challengesSuccess[type] * 3;
    }

    // =====================================================
    // GETTERS FOR UI
    // =====================================================

    public int GetObjects(ProfessionType type)
    {
        return objectsCollected.TryGetValue(type, out int value)
            ? value
            : 0;
    }

    public int GetPortals(ProfessionType type)
    {
        return portalsActivated.TryGetValue(type, out int value)
            ? value
            : 0;
    }

    public int GetSuccessChallenges(ProfessionType type)
    {
        return challengesSuccess.TryGetValue(type, out int value)
            ? value
            : 0;
    }
    public int GetFailures(ProfessionType type)
    {
        return challengesFailed.TryGetValue(type, out int value)
            ? value
            : 0;
    }
    public int GetTotalRuns()
    {
        return totalRuns;
    }

    public int GetTotalSuccessChallenges()
    {
        int total = 0;

        foreach (var value in challengesSuccess.Values)
            total += value;

        return total;
    }

    // =====================================================
    // SAVE / LOAD
    // =====================================================

    private void SaveStats()
    {
        if (!autoSave)
            return;

        foreach (var kv in objectsCollected)
            PlayerPrefs.SetInt($"pref_obj_{kv.Key}", kv.Value);

        foreach (var kv in portalsActivated)
            PlayerPrefs.SetInt($"pref_portal_{kv.Key}", kv.Value);

        foreach (var kv in challengesSuccess)
            PlayerPrefs.SetInt($"pref_success_{kv.Key}", kv.Value);
        foreach (var kv in challengesFailed)
            PlayerPrefs.SetInt($"pref_fail_{kv.Key}", kv.Value);

        PlayerPrefs.SetInt("pref_total_runs", totalRuns);

        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        foreach (ProfessionType type in Enum.GetValues(typeof(ProfessionType)))
        {
            if (type == ProfessionType.None)
                continue;

            objectsCollected[type] =
                PlayerPrefs.GetInt($"pref_obj_{type}", 0);

            portalsActivated[type] =
                PlayerPrefs.GetInt($"pref_portal_{type}", 0);

            challengesSuccess[type] =
                PlayerPrefs.GetInt($"pref_success_{type}", 0);
            challengesFailed[type] =
    PlayerPrefs.GetInt($"pref_fail_{type}", 0);
        }

        totalRuns =
            PlayerPrefs.GetInt("pref_total_runs", 0);

        Debug.Log(
    $"[PreferenceAnalyzer] Loaded. " +
    $"Runs={totalRuns}, " +
    $"Successes={GetTotalSuccessChallenges()}"
);
    }

    // =====================================================
    // RESET
    // =====================================================

    public void ResetAllStats()
    {
        InitDictionaries();

        totalRuns = 0;
        dominantProfession = ProfessionType.None;

        SaveStats();
    }
}