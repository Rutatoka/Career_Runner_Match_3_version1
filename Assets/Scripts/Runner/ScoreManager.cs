using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float Score { get; private set; }
    private float baseMultiplier = 1f;
    private bool isActive = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isActive) return;

        float comboMult = ComboSystem.Instance != null ? ComboSystem.Instance.GetMultiplier() : 1f;

        Score += Time.deltaTime * baseMultiplier * comboMult;
    }

    public void SetBaseMultiplier(float value)
    {
        baseMultiplier = value;
    }

    public void ResetBaseMultiplier()
    {
        baseMultiplier = 1f;
    }

    public void PauseScore()
    {
        isActive = false;
    }
    public void SetMultiplier(float value)
    {
        SetBaseMultiplier(value);
    }

    public void ResetMultiplier()
    {
        ResetBaseMultiplier();
    }

    public void ResumeScore()
    {
        isActive = true;
    }

    public void ResetScore()
    {
        Score = 0f;
    }
}
