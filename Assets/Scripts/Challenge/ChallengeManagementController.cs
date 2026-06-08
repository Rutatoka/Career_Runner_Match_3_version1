using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ChallengeManagementController : MonoBehaviour
{
    [Header("Settings")]
    public int requiredCorrect = 4; // 4 листов

    [Header("References")]
    public ResultWindow resultWindow;
    public List<FolderButton> folderButtons; // Три папки-кнопки (Красная, Желтая, Зеленая)

    [Header("UI")]
    public TMP_Text taskDescriptionText;
    public TMP_Text feedbackText;
    public GameObject taskPanel;
    public TMP_Text progressText;
    private bool ended;
    [Header("Content")]
    [TextArea(5, 10)]
    public List<string> taskDescriptions; // 5 описаний задач

    private int currentSheetIndex = 0;
    private int correctAnswers = 0;
    private bool finished = false;
    private bool isWaitingForSelection = false;

    private void Start()
    {
        if (resultWindow == null)
            resultWindow = FindObjectOfType<ResultWindow>();

        InitializeGame();
    }

    private void InitializeGame()
    {
        currentSheetIndex = 0;
        correctAnswers = 0;
        finished = false;

        foreach (var button in folderButtons)
        {
            button.SetInteractable(true);
            button.ResetButton();
        }

        ClearFeedback();
        ShowNextSheet();
        UpdateUI();
    }

    private void ShowNextSheet()
    {
        if (taskPanel != null)
            taskPanel.SetActive(true);

        if (taskDescriptions.Count > currentSheetIndex)
        {
            taskDescriptionText.text = taskDescriptions[currentSheetIndex];
        }

        isWaitingForSelection = true;
        Debug.Log($"[ChallengeManagement] Показан лист {currentSheetIndex + 1}");
    }

    public void OnFolderClicked(ColorType folderColor)
    {
        if (finished || !isWaitingForSelection) return;

        Debug.Log($"[ChallengeManagement] Выбрана папка: {folderColor}");

        bool isCorrect = IsCorrectAnswer(currentSheetIndex, folderColor);

        foreach (var btn in folderButtons)
            btn.SetInteractable(false);

        if (isCorrect)
            correctAnswers++;

        ShowFeedback(isCorrect, currentSheetIndex, folderColor);

        StartCoroutine(ProcessNextStepAfterFeedback());
    }

    private IEnumerator ProcessNextStepAfterFeedback()
    {
        yield return new WaitForSeconds(4.5f);

        currentSheetIndex++;

        // Показываем следующий лист, если он есть
        if (currentSheetIndex < taskDescriptions.Count)
        {
            ClearFeedback();
            ShowNextSheet();
            UpdateUI();

            foreach (var btn in folderButtons)
                btn.SetInteractable(true);
        }
        else
        {
            // Все листы показаны - проверяем результат
            if (correctAnswers >= requiredCorrect)
            {
                FinishChallenge(true);
            }
            else
            {
                FinishChallenge(false);
            }
        }
    }

    private bool IsCorrectAnswer(int sheetIndex, ColorType chosenColor)
    {
        // Ситуация 0: Сервер упал ? Красная
        if (sheetIndex == 0) return chosenColor == ColorType.Red;

        // Ситуация 1: Презентация инвесторам ? Желтая
        if (sheetIndex == 1) return chosenColor == ColorType.Yellow;

        // Ситуация 2: Таблица продаж ? Зеленая
        if (sheetIndex == 2) return chosenColor == ColorType.Green;

        // Ситуация 3: Рекламный баннер ? Красная
        if (sheetIndex == 3) return chosenColor == ColorType.Red;

        // Ситуация 4: Отчет для руководителя ? Желтая
        if (sheetIndex == 4) return chosenColor == ColorType.Yellow;

        return false;
    }

    private void ShowFeedback(bool isCorrect, int sheetIndex, ColorType chosenColor)
    {
        string message = "";
        Color color = isCorrect ? Color.green : Color.red;

        // --- Ситуация 0: Сервер упал ---
        if (sheetIndex == 0)
        {
            if (chosenColor == ColorType.Red)
                message = isCorrect ? "Правильно! Сервер упал — это срочно и важно. Клиенты теряют доступ, нужно действовать немедленно."
                                    : "Ошибка! Сервер упал — это срочно и важно, а ты выбрал другую папку.";
            else if (chosenColor == ColorType.Yellow)
                message = "Неверно. Это не 'важно, но не срочно'. Клиенты уже не могут войти — это горит!";
            else
                message = "Ошибка. Ты положил аварию на сервере в 'не важно, не срочно'. Это катастрофа.";
        }
        // --- Ситуация 1: Презентация инвесторам ---
        else if (sheetIndex == 1)
        {
            if (chosenColor == ColorType.Red)
                message = "Неверно. Презентация важна, но она не требует немедленного решения прямо сейчас. У тебя есть время до завтра.";
            else if (chosenColor == ColorType.Yellow)
                message = isCorrect ? "Правильно! Презентация важна для будущего компании, но у тебя есть время до утра. Ты верно расставил приоритеты."
                                    : "Ошибка! Презентация важна, но не срочно. Выбрана не та папка.";
            else
                message = "Ошибка. Ты положил важную презентацию для инвесторов в 'не важно'. Это может стоить компании финансирования.";
        }
        // --- Ситуация 2: Таблица продаж ---
        else if (sheetIndex == 2)
        {
            if (chosenColor == ColorType.Red)
                message = "Неверно. Это не срочно. Квартал только через месяц.";
            else if (chosenColor == ColorType.Yellow)
                message = "Неверно. Это не важно для текущих задач. Таблицу можно отложить.";
            else
                message = isCorrect ? "Правильно! Это действительно не важно и не срочно. Можно сделать в свободное время."
                                    : "Ошибка! Таблица не важна и не срочна.";
        }
        // --- Ситуация 3: Рекламный баннер ---
        else if (sheetIndex == 3)
        {
            if (chosenColor == ColorType.Red)
                message = isCorrect ? "Правильно! Баннер запускается через 2 часа, ошибка в тексте — это срочно и важно для репутации."
                                    : "Ошибка! Баннер запускается через 2 часа, а ты выбрал другую папку.";
            else if (chosenColor == ColorType.Yellow)
                message = "Неверно. Это не 'важно, но не срочно'. Баннер выходит уже через 2 часа, надо действовать сейчас.";
            else
                message = "Ошибка. Ты положил срочный баннер в 'не важно, не срочно'. Репутация компании под угрозой.";
        }
        // --- Ситуация 4: Отчет для руководителя ---
        else if (sheetIndex == 4)
        {
            if (chosenColor == ColorType.Red)
                message = "Неверно. Отчет нужен через две недели — это не срочно. Не надо делать его прямо сейчас.";
            else if (chosenColor == ColorType.Yellow)
                message = isCorrect ? "Правильно! Отчет важен для руководителя, но у тебя есть две недели. Ты верно расставил приоритеты."
                                    : "Ошибка! Отчет важен, но не срочен.";
            else
                message = "Неверно. Отчет важен для руководителя. Нельзя класть его в 'не важно'.";
        }

        feedbackText.text = message;
        feedbackText.color = color;
    }

    private void ClearFeedback()
    {
        feedbackText.text = "";
    }

    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Лист {currentSheetIndex + 1}/{taskDescriptions.Count}";
    }

    private void FinishChallenge(bool success)
    {
        if (ended) return;
        ended = true;

        finished = true;

        Debug.Log($"[ChallengeManagement] EndChallenge, success={success}");

        if (taskPanel != null)
            taskPanel.SetActive(false);

        foreach (var btn in folderButtons)
            btn.SetInteractable(false);

        ChallengeManager.Instance?.FinishChallenge(success);

        StartCoroutine(ShowResultDelayed(success));
    }
    private IEnumerator ShowResultDelayed(bool success)
    {
        yield return new WaitForSeconds(0.5f);

        if (resultWindow == null)
            yield break;

        if (success)
        {
            resultWindow.ShowSuccess(correctAnswers, requiredCorrect, 0);
        }
        else
        {
            resultWindow.ShowFailure(correctAnswers, requiredCorrect, "wrong");
        }
    }
}