using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ChallengeMarketingController : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 20f;
    public int requiredGood = 3;  // Нужно 3 хороших для победы
    public int maxBad = 3;        // Максимум 3 плохих до поражения
    private bool ended;

    [Header("References")]
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public ResultWindow resultWindow;
    public IconSpawner spawner;

    private float timeLeft;
    private int goodCount = 0;
    private int badCount = 0;
    private bool finished = false;

    private void Start()
    {
        timeLeft = duration;
        UpdateUI();
        spawner.StartSpawning();
ClearFeedback();
    }

    private void Update()
    {
        if (ended) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Fail("Время вышло!");
            return;
        }

        UpdateUI();
    }

    public void CatchGood()
    {
        if (finished) return;
        if (ended) return;
        goodCount++;
        ShowFeedback($"Отлично! {goodCount}/{requiredGood}", Color.green);

        if (goodCount >= requiredGood)
        {
            Success();
        }

        UpdateUI();
    }

    public void CatchBad()
    {
        if (finished) return;

        badCount++;
        ShowFeedback($"Плохой комментарий! {badCount}/{maxBad}", Color.red);

        if (badCount >= maxBad)
        {
            Fail("Слишком много плохих комментариев!");
        }

        UpdateUI();
    }

    private void Success()
    {
        EndChallenge(true, "success");
    }
    private void EndChallenge(bool success, string reason = "")
    {
        if (ended) return;
        ended = true;
        finished = true;

        spawner.StopSpawning();

        if (success)
            ChallengeManager.Instance?.FinishChallenge(true);
        else
            ChallengeManager.Instance?.FailChallenge(reason);

        if (resultWindow != null)
        {
            if (success)
                resultWindow.ShowSuccess(goodCount, requiredGood, timeLeft);
            else
                resultWindow.ShowFailure(goodCount, requiredGood, reason);
        }
    }
    private void Fail(string reason)
    {
        EndChallenge(false, reason);
    }

    private void UpdateUI()
    {
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        progressText.text = $" {goodCount}/{requiredGood} |  {badCount}/{maxBad}";
    }

    private void ShowFeedback(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;
        CancelInvoke(nameof(ClearFeedback));
        Invoke(nameof(ClearFeedback), 1.5f);
    }

    private void ClearFeedback()
    {
        feedbackText.text = "";
    }
}