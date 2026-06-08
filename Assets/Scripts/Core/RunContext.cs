using UnityEngine;

public class RunContext : MonoBehaviour
{
    public static RunContext Instance;

    public ProfessionType CurrentProfession;
    public bool IsRunActive;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartRun(ProfessionType type)
    {
        CurrentProfession = type;
        IsRunActive = true;

        Debug.Log($"[RunContext] START: {type}");
    }

    public void FinishRun()
    {
        IsRunActive = false;

        Debug.Log($"[RunContext] FINISH: {CurrentProfession}");
    }
}