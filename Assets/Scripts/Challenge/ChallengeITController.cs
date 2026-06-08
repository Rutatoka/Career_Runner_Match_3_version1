using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ChallengeITController : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 20f;
    public int requiredCorrect = 3;
    public int maxAttempts = 5;

    [Header("References")]
    public List<DropZoneIT> dropZones;
    public ResultWindow resultWindow;
    private bool ended;
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;

    private int attempts = 0;
    private float timeLeft;
    private bool finished = false;

    private void Start()
    {
        Debug.Log("[ChallengeIT] Start");
        timeLeft = duration;

        // Находим все DropZone
        if (dropZones == null || dropZones.Count == 0)
        {
            dropZones = new List<DropZoneIT>(FindObjectsOfType<DropZoneIT>());
        }

        // Находим ResultWindow если не назначен
        if (resultWindow == null)
        {
            resultWindow = FindObjectOfType<ResultWindow>();
        }

        UpdateUI();
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Debug.Log("[ChallengeIT] Time is over → fail");
            EndChallenge(false, "time");
            return;
        }

        UpdateUI();
        CheckAllZonesFilled();
    }

    public void OnBlockDropped(bool isCorrect)
    {
        if (finished)
        {
            Debug.Log("[ChallengeIT] OnBlockDropped called but challenge already finished");
            return;
        }

        attempts++;

        if (isCorrect)
        {
            ShowFeedback("Правильно!", Color.green);
        }
        else
        {
            ShowFeedback("Неправильный блок!", Color.red);
        }

        // Считаем актуальное количество правильных блоков
        int currentCorrectBlocks = CountCorrectBlocks();
        int totalOccupiedZones = CountOccupiedZones();

        Debug.Log($"[ChallengeIT] OnBlockDropped: isCorrect={isCorrect}, " +
                  $"attempts={attempts}/{maxAttempts}, " +
                  $"correct blocks in zones={currentCorrectBlocks}/{requiredCorrect}, " +
                  $"occupied zones={totalOccupiedZones}/{dropZones.Count}");

        UpdateUI();
        CheckAllZonesFilled();

        // Проверяем условия завершения
        if (attempts >= maxAttempts)
        {
            bool allCorrect = AreAllBlocksCorrect();
            Debug.Log($"[ChallengeIT] Max attempts reached, all correct: {allCorrect}");

            if (allCorrect)
            {
                EndChallenge(true, "success");
            }
            else
            {
                ChallengeManager.Instance?.FailChallenge("wrong");

                EndChallenge(false, "wrong");
            }
            return;
        }
    }

    public void OnBlockExtracted()
    {
        if (finished) return;

        // Не уменьшаем attempts, так как это не новая попытка
        // Просто обновляем UI и проверяем статус

        int currentCorrectBlocks = CountCorrectBlocks();
        int totalOccupiedZones = CountOccupiedZones();

        Debug.Log($"[ChallengeIT] Block extracted. " +
                  $"correct blocks={currentCorrectBlocks}/{requiredCorrect}, " +
                  $"occupied zones={totalOccupiedZones}/{dropZones.Count}");

        UpdateUI();
    }

    private void CheckAllZonesFilled()
    {
        if (finished) return;

        // Проверяем, все ли зоны заняты
        int occupiedZones = CountOccupiedZones();

        if (occupiedZones >= dropZones.Count)
        {
            // Все зоны заполнены, проверяем правильность
            bool allCorrect = AreAllBlocksCorrect();
            int correctBlocks = CountCorrectBlocks();

            Debug.Log($"[ChallengeIT] All zones filled! " +
                      $"Correct blocks: {correctBlocks}/{requiredCorrect}, " +
                      $"All correct: {allCorrect}");

            if (allCorrect)
            {
                Debug.Log("[ChallengeIT] All blocks are correct - SUCCESS!");
                EndChallenge(true, "success");
            }
            else
            {
                Debug.Log("[ChallengeIT] Not all blocks are correct yet");
                ShowFeedback($"Правильно: {correctBlocks}/{requiredCorrect}. Проверьте остальные блоки!", Color.yellow);
            }
        }
    }

    // Считает количество занятых зон
    private int CountOccupiedZones()
    {
        if (dropZones == null) return 0;

        int count = 0;
        foreach (var zone in dropZones)
        {
            if (zone != null && zone.IsOccupied())
            {
                count++;
            }
        }
        return count;
    }

    // Считает количество ПРАВИЛЬНЫХ блоков в зонах
    private int CountCorrectBlocks()
    {
        if (dropZones == null) return 0;

        int count = 0;
        foreach (var zone in dropZones)
        {
            if (zone != null && zone.IsOccupied() && zone.HasCorrectBlock())
            {
                count++;
            }
        }
        return count;
    }

    // Проверяет, все ли блоки в зонах правильные
    private bool AreAllBlocksCorrect()
    {
        if (dropZones == null || dropZones.Count == 0) return false;

        foreach (var zone in dropZones)
        {
            if (zone == null || !zone.IsOccupied() || !zone.HasCorrectBlock())
            {
                return false;
            }
        }
        return true;
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            // Останавливаем предыдущее скрытие
            CancelInvoke(nameof(ClearFeedback));

            feedbackText.text = message;
            feedbackText.color = color;

            // Автоматически скрываем через 3 секунды
            Invoke(nameof(ClearFeedback), 3f);
        }
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (progressText != null)
        {
            int correctBlocks = CountCorrectBlocks();
            progressText.text = $"{correctBlocks}/{requiredCorrect}";
        }
    }

    private void EndChallenge(bool success, string reason = "")
    {
        if (ended) return;
        ended = true;

        finished = true;

        Debug.Log($"[ChallengeIT] EndChallenge success={success}, reason={reason}");

        DisableAllDropZones();

        int finalCorrectBlocks = CountCorrectBlocks();

        ChallengeManager.Instance?.FinishChallenge(success);

        StartCoroutine(
            ShowResultDelayed(
                success,
                reason,
                finalCorrectBlocks
            )
        );
    }

    private System.Collections.IEnumerator ShowResultDelayed(bool success, string reason, int correctBlocks)
    {
        // Небольшая задержка для анимации последнего блока
        yield return new WaitForSeconds(0.5f);

        if (resultWindow != null)
        {
            if (success)
            {
                resultWindow.ShowSuccess(correctBlocks, requiredCorrect, timeLeft);
            }
            else
            {
                resultWindow.ShowFailure(correctBlocks, requiredCorrect, reason);

            }
        }
        else
        {
            Debug.LogWarning("[ChallengeIT] ResultWindow not found!");
        }
    }

    private void DisableAllDropZones()
    {
        if (dropZones == null) return;

        foreach (var zone in dropZones)
        {
            if (zone != null)
            {
                zone.enabled = false;
            }
        }
    }
}