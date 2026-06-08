using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ChallengeDesignController : MonoBehaviour
{
    [Header("Challenge Settings")]
    public float duration = 25f;
    public int requiredCorrect = 3;
    public int maxAttempts = 5;
    public float autoSubmitDelay = 1.5f;

    [Header("References")]
    public List<DropZoneDesign> dropZones;
    public ResultWindow resultWindow;
    public List<DraggableColorBlock> colorBlocks;

    [Header("UI Elements")]
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;

    [Header("Visual Feedback")]
    public GameObject successParticles;
    public GameObject failParticles;
    private bool ended;
    private int attempts = 0;
    private int correctPlacements = 0;
    private float timeLeft;
    private bool finished = false;
    private Coroutine feedbackCoroutine;

 
    private void Start()
    {
        timeLeft = duration;

        if (dropZones == null || dropZones.Count == 0)
            dropZones = new List<DropZoneDesign>(FindObjectsOfType<DropZoneDesign>());

        if (resultWindow == null)
            resultWindow = FindObjectOfType<ResultWindow>();

        if (colorBlocks == null || colorBlocks.Count == 0)
            colorBlocks = new List<DraggableColorBlock>(FindObjectsOfType<DraggableColorBlock>());

        UpdateUI();
        StartCoroutine(IntroAnimation());
    }

    private IEnumerator IntroAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        ShowFeedback("Создайте гармоничную композицию постера!", Color.white);
        yield return new WaitForSeconds(2f);
        ClearFeedback();
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            // ВРЕМЯ ИСТЕКЛО - ПРОИГРЫШ
            ShowFeedback("Время вышло!", Color.red);
            EndChallenge(false, "time");
            return;
        }

        UpdateUI();
    }

    public void OnColorDropped(bool isCorrect)
    {
        if (finished) return;

        attempts++;

        if (isCorrect)
        {
            correctPlacements++;
            ShowFeedback("Отличный выбор! Цвета сочетаются!", Color.green);

            if (successParticles != null && correctPlacements - 1 < dropZones.Count)
            {
                GameObject particles = Instantiate(successParticles,
                    dropZones[correctPlacements - 1].transform.position,
                    Quaternion.identity);
                Destroy(particles, 2f);
            }
        }
        else
        {
            ShowFeedback("Этот цвет нарушает гармонию композиции", Color.red);
            StartCoroutine(ShowHarmonyHintDelayed());
        }

        UpdateUI();

        // Проверка: все зоны заполнены
        if (dropZones.TrueForAll(z => z.IsOccupied()))
        {
            if (AreAllCorrect())
            {
                // ВСЕ ПРАВИЛЬНО - ПОБЕДА
                ShowFeedback("Превосходно! Идеальная композиция!", Color.green);
                StartCoroutine(DelayedWin());
            }
            else
            {
                // ЗАПОЛНЕНО НО С ОШИБКАМИ
                ShowFeedback("Почти готово! Попробуйте другие сочетания", Color.yellow);
            }
        }

        // Проверка: превышен лимит попыток
        if (attempts >= maxAttempts)
        {
            if (AreAllCorrect())
            {
                // ЛИМИТ ИСЧЕРПАН, НО ВСЕ ПРАВИЛЬНО - ПОБЕДА
                ShowFeedback("Успех! Все цвета на месте!", Color.green);
                StartCoroutine(DelayedWin());
            }
            else
            {
                // ЛИМИТ ИСЧЕРПАН С ОШИБКАМИ - ПРОИГРЫШ
                ShowFeedback("Слишком много попыток!", Color.red);
                StartCoroutine(DelayedLose("attempts"));
            }
        }
    }

    private IEnumerator DelayedWin()
    {
        yield return new WaitForSeconds(autoSubmitDelay);
        if (!finished)
        {
            EndChallenge(true, "success");
        }
    }

    private IEnumerator DelayedLose(string reason)
    {
        yield return new WaitForSeconds(autoSubmitDelay);
        if (!finished)
        {
            EndChallenge(false, reason);
        }
    }

    private IEnumerator ShowHarmonyHintDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (!finished)
        {
            string[] hints = {
                "💡 Попробуйте дополнительные цвета",
                "💡 Используйте цвета одной температуры",
                "💡 Сочетайте светлые и темные оттенки",
                "💡 Выберите цвета из одного сектора палитры"
            };
            ShowFeedback(hints[Random.Range(0, hints.Length)], Color.yellow);
        }
    }

    private bool AreAllCorrect()
    {
        foreach (var zone in dropZones)
        {
            if (!zone.HasCorrectBlock())
                return false;
        }
        return true;
    }

    public void SubmitChallenge()
    {
        if (finished) return;

        if (AreAllCorrect() && correctPlacements >= requiredCorrect)
        {
            // Ручная отправка - ПОБЕДА
            ShowFeedback("🎉 Великолепная работа! Настоящий дизайнер!", Color.green);
            EndChallenge(true, "success");
        }
        else
        {
            // Ручная отправка - ПРОИГРЫШ
            ShowFeedback("😔 Композицию можно улучшить", Color.red);
            EndChallenge(false, "wrong");
        }
    }

    public void ResetChallenge()
    {
        if (finished) return;

        foreach (var zone in dropZones)
        {
            zone.Clear();
        }

        foreach (var block in colorBlocks)
        {
            block.ResetBlock();
        }

        attempts = 0;
        correctPlacements = 0;
        ClearFeedback();
        UpdateUI();
    }

    public void OnBlockRemoved(string colorId)
    {
        if (finished) return;

        correctPlacements = CountCorrectZones();
        ShowFeedback("↩️ Цвет возвращен в палитру", Color.cyan);
        UpdateUI();
    }

    private int CountCorrectZones()
    {
        int count = 0;
        foreach (var zone in dropZones)
        {
            if (zone.IsOccupied() && zone.HasCorrectBlock())
                count++;
        }
        return count;
    }

    private void ShowFeedback(string msg, Color color)
    {
        if (feedbackText == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackText.text = msg;
        feedbackText.color = color;

        StartCoroutine(PulseText());
        feedbackCoroutine = StartCoroutine(ClearFeedbackDelayed(3f));
    }

    private IEnumerator PulseText()
    {
        Vector3 originalScale = feedbackText.transform.localScale;
        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            feedbackText.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.1f, t);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            feedbackText.transform.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t);
            yield return null;
        }

        feedbackText.transform.localScale = originalScale;
    }

    private IEnumerator ClearFeedbackDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearFeedback();
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

            if (timeLeft <= 5f)
                timerText.color = Color.red;
            else if (timeLeft <= 10f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }

        if (progressText != null)
            progressText.text = $"{correctPlacements}/{requiredCorrect}";
    }

    private void EndChallenge(bool success, string reason = "")
    {
        if (ended) return;
        ended = true;

        finished = true;

        Debug.Log($"[Design] END → success={success}, reason={reason}");

        ChallengeManager.Instance?.FinishChallenge(success);

        StopAllCoroutines();

        foreach (var zone in dropZones)
            zone.enabled = false;

        foreach (var block in colorBlocks)
            block.enabled = false;

        StartCoroutine(ShowResultDelayed(success, reason, correctPlacements));
    }

    private IEnumerator ShowResultDelayed(bool success, string reason, int correct)
    {
        yield return new WaitForSeconds(1f);

        if (resultWindow != null)
        {
            if (success)
            {
                resultWindow.ShowSuccess(correct, requiredCorrect, timeLeft);
            }
            else
            {
                resultWindow.ShowFailure(correct, requiredCorrect, reason);
            }
        }
    }
}