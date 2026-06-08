using UnityEngine;
using System;

public class ProfessionSystem : MonoBehaviour
{
    public static ProfessionSystem Instance;

    [Header("Settings")]
    public int requiredCount = 3;

    [Header("Debug")]
    public ProfessionType currentType = ProfessionType.None;
    public int currentCount = 0;

    public event Action<ProfessionType, int> OnProgressChanged;
    public event Action<ProfessionType> OnPortalReady;
    public event Action<ProfessionType> OnPortalActivated;
    public event System.Action<ProfessionType> OnProfessionCollected;
    public ProfessionType CurrentProfession => currentType;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    
}

    public void CollectProfessionObject(ProfessionObjectData data)
    {
        if (data == null) return;

        var type = data.professionType;
        SlowMoController.Instance?.ForceEndSlowMo();

        // Показываем мысль персонажа ПРИ КАЖДОМ СБОРЕ
        string thought = data.GetRandomThought();
        if (!string.IsNullOrEmpty(thought) && ThoughtBubble.Instance != null)
        {
            ThoughtBubble.Instance.Show(thought);
        }

        // 1. Первый объект
        if (currentType == ProfessionType.None)
        {
            currentType = type;
            currentCount = 1;

            NotifyProgress();
            OnProfessionCollected?.Invoke(type);
            return;
        }

        // 2. Другой тип → сброс
        if (type != currentType)
        {
            ResetProgress();
            currentType = type;
            currentCount = 1;

            NotifyProgress();
            OnProfessionCollected?.Invoke(type);
            return;
        }

        // 3. Тот же тип
        currentCount++;
        NotifyProgress();
        OnProfessionCollected?.Invoke(type);

        // 4. Портал
        if (currentCount >= requiredCount)
            ActivatePortal(type);
    }

    public void ResetProgress()
    {
        currentType = ProfessionType.None;
        currentCount = 0;
        NotifyProgress();
    }

    private void NotifyProgress()
    {
        OnProgressChanged?.Invoke(currentType, currentCount);
    }

    private void ActivatePortal(ProfessionType type)
    {
        OnPortalReady?.Invoke(type);

        SlowMoController.Instance?.TriggerPortalSlowMo();
        PortalSystem.Instance?.ShowPortal(type);
        PreferenceAnalyzer.Instance?.
    RegisterPortal(type);
        OnPortalActivated?.Invoke(type);

        ResetProgress();
    }
    public ProfessionType GetDominantType()
    {
        return currentType;
    }
}
