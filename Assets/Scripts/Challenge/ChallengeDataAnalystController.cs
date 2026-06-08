using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ChallengeDataAnalystController : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 30f;
    public int requiredCorrect = 3;
    public int maxAttempts = 5;

    [Header("Difficulty Settings")]
    public bool useProgressiveDifficulty = true;
    public int easyPatternsCount = 2;
    public int mediumPatternsCount = 2;
    public int hardPatternsCount = 1;

    [Header("References")]
    public ResultWindow resultWindow;
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text feedbackText;
    public TMP_Text sequenceText;
    public TMP_Text questionText;
    public TMP_Text difficultyText;

    [Header("Answer Buttons")]
    public List<AnswerButton> answerButtons;

    [Header("Patterns Database")]
    public List<PatternData> patterns;

    private int attempts = 0;
    private int correctAnswers = 0;
    private float timeLeft;
    private bool finished = false;

    private PatternData currentPattern;
    private List<char> currentSequence;
    private char correctAnswer;

    private List<PatternData> availablePatterns;
    private List<PatternData> usedPatterns;
    private int currentDifficultyLevel = 1;

    [System.Serializable]
    public class PatternData
    {
        public string name;
        public string description;
        public PatternType type;
        public int difficulty;

        [TextArea(1, 3)]
        public string sequenceExample;
        public char correctAnswer;
        public List<char> wrongAnswers;
    }

    public enum PatternType
    {
        Arithmetic,
        Geometric,
        Alternating,
        Palindrome,
        Reverse,
        Skip,
        Fibonacci
    }

    [System.Serializable]
    public class AnswerButton
    {
        public Button button;
        public TMP_Text buttonText;
        public char symbol;

        public void SetAnswer(char answer)
        {
            symbol = answer;
            if (buttonText != null)
                buttonText.text = answer.ToString();
        }
    }

    [ContextMenu("Generate Default Patterns")]
    public void GenerateDefaultPatterns()
    {
        patterns = new List<PatternData>
    {
        // Уровень 1 (лёгкие)
        new PatternData
        {
            name = "Арифметика букв",
            description = "Найдите закономерность в последовательности букв",
            type = PatternType.Arithmetic,
            difficulty = 1,
            sequenceExample = "А, В, Д, Ё, ?",
            correctAnswer = 'З',
            wrongAnswers = new List<char> { 'Ж', 'И', 'Е' }
        },
        new PatternData
        {
            name = "Числовая прогрессия",
            description = "Найдите закономерность в числах",
            type = PatternType.Arithmetic,
            difficulty = 1,
            sequenceExample = "2, 4, 6, 8, ?",
            correctAnswer = '0', // 10, но берём первый символ
            wrongAnswers = new List<char> { '1', '9', '7' }
        },
        new PatternData
        {
            name = "Обратный порядок",
            description = "Последовательность идёт в обратном направлении",
            type = PatternType.Reverse,
            difficulty = 1,
            sequenceExample = "5, 4, 3, 2, ?",
            correctAnswer = '1',
            wrongAnswers = new List<char> { '0', '6', '3' }
        },
        
        // Уровень 2 (средние)
        new PatternData
        {
            name = "Геометрическая с буквами",
            description = "Расстояние между буквами увеличивается",
            type = PatternType.Geometric,
            difficulty = 2,
            sequenceExample = "А, Б, Г, Ё, ?",
            correctAnswer = 'Л',
            wrongAnswers = new List<char> { 'К', 'М', 'И' }
        },
        new PatternData
        {
            name = "Чередование",
            description = "Буквы берутся с разных концов алфавита",
            type = PatternType.Alternating,
            difficulty = 2,
            sequenceExample = "А, Я, Б, Ю, В, ?",
            correctAnswer = 'Э',
            wrongAnswers = new List<char> { 'Г', 'Ь', 'Ы' }
        },
        new PatternData
        {
            name = "Умножение чисел",
            description = "Каждое следующее число умножается на 2",
            type = PatternType.Geometric,
            difficulty = 2,
            sequenceExample = "1, 2, 4, 8, ?",
            correctAnswer = '6', // 16, берём первую цифру
            wrongAnswers = new List<char> { '1', '3', '4' }
        },
        
        // Уровень 3 (сложные)
        new PatternData
        {
            name = "Пропуск букв",
            description = "Каждый раз пропускается на одну букву больше",
            type = PatternType.Skip,
            difficulty = 3,
            sequenceExample = "А, В, Е, К, ?",
            correctAnswer = 'П',
            wrongAnswers = new List<char> { 'О', 'Р', 'Л' }
        },
        new PatternData
        {
            name = "Фибоначчи числа",
            description = "Каждое число - сумма двух предыдущих",
            type = PatternType.Fibonacci,
            difficulty = 3,
            sequenceExample = "1, 1, 2, 3, ?",
            correctAnswer = '5',
            wrongAnswers = new List<char> { '4', '6', '8' }
        },
        new PatternData
        {
            name = "Смешанная последовательность",
            description = "Буквы чередуются с цифрами по правилу",
            type = PatternType.Skip,
            difficulty = 3,
            sequenceExample = "1, Б, 3, Г, 5, ?",
            correctAnswer = 'Е',
            wrongAnswers = new List<char> { 'Д', 'Ё', 'Ж' }
        },
    };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"Default patterns generated: {patterns.Count} patterns");
    }
    private void Start()
    {
        Debug.Log("[DataAnalyst] Start");
        timeLeft = duration;

        if (resultWindow == null)
        {
            resultWindow = FindObjectOfType<ResultWindow>();
        }

        if (answerButtons == null || answerButtons.Count == 0)
        {
            Debug.LogError("[DataAnalyst] Answer buttons are not assigned in inspector!");
            return;
        }

        SetupButtons();
        InitializePatternPool();
        LoadNewPattern();
        UpdateUI();
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            Debug.Log("[DataAnalyst] Time is over → fail");
            EndChallenge(false, "time");
            return;
        }

        UpdateUI();
    }

    private void SetupButtons()
    {
        for (int i = 0; i < answerButtons.Count; i++)
        {
            int index = i;
            if (answerButtons[i].button != null)
            {
                answerButtons[i].button.onClick.RemoveAllListeners();
                answerButtons[i].button.onClick.AddListener(() => OnAnswerSelected(index));
            }
        }
    }

    private void InitializePatternPool()
    {
        if (patterns == null || patterns.Count == 0)
        {
            Debug.LogError("[DataAnalyst] No patterns in database!");
            return;
        }

        if (useProgressiveDifficulty)
        {
            InitializeProgressivePool();
        }
        else
        {
            InitializeRandomPool();
        }

        Debug.Log($"[DataAnalyst] Pattern pool initialized: {availablePatterns.Count} patterns");
        for (int i = 0; i < availablePatterns.Count; i++)
        {
            Debug.Log($"  {i + 1}. {availablePatterns[i].name} (difficulty: {availablePatterns[i].difficulty})");
        }
    }

    private void InitializeProgressivePool()
    {
        availablePatterns = new List<PatternData>();
        usedPatterns = new List<PatternData>();

        var easyPatterns = patterns.Where(p => p.difficulty == 1).ToList();
        var mediumPatterns = patterns.Where(p => p.difficulty == 2).ToList();
        var hardPatterns = patterns.Where(p => p.difficulty == 3).ToList();

        ShuffleList(easyPatterns);
        ShuffleList(mediumPatterns);
        ShuffleList(hardPatterns);

        Debug.Log($"[DataAnalyst] Available - Easy: {easyPatterns.Count}, Medium: {mediumPatterns.Count}, Hard: {hardPatterns.Count}");

        availablePatterns.AddRange(easyPatterns.Take(easyPatternsCount));
        availablePatterns.AddRange(mediumPatterns.Take(mediumPatternsCount));
        availablePatterns.AddRange(hardPatterns.Take(hardPatternsCount));

        if (availablePatterns.Count < requiredCorrect)
        {
            var usedPatternNames = availablePatterns.Select(p => p.name).ToHashSet();
            var remaining = patterns.Where(p => !usedPatternNames.Contains(p.name)).ToList();
            ShuffleList(remaining);

            int needed = requiredCorrect - availablePatterns.Count;
            availablePatterns.AddRange(remaining.Take(needed));

            Debug.LogWarning($"[DataAnalyst] Added {needed} extra patterns to meet required count");
        }
    }

    private void InitializeRandomPool()
    {
        availablePatterns = new List<PatternData>(patterns);
        usedPatterns = new List<PatternData>();
        ShuffleList(availablePatterns);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void LoadNewPattern()
    {
        if (availablePatterns == null || availablePatterns.Count == 0)
        {
            Debug.LogWarning("[DataAnalyst] No more patterns available!");

            if (useProgressiveDifficulty && correctAnswers < requiredCorrect)
            {
                Debug.LogError("[DataAnalyst] Not enough unique patterns for progressive mode!");
                EndChallenge(true, "success");
                return;
            }
            else
            {
                Debug.Log("[DataAnalyst] Resetting pattern pool");
                usedPatterns.Clear();
                availablePatterns = new List<PatternData>(patterns);
                ShuffleList(availablePatterns);
            }
        }

        currentPattern = availablePatterns[0];
        availablePatterns.RemoveAt(0);
        usedPatterns.Add(currentPattern);

        currentDifficultyLevel = currentPattern.difficulty;

        string[] parts = currentPattern.sequenceExample.Split(',');
        currentSequence = new List<char>();

        for (int i = 0; i < parts.Length - 1; i++)
        {
            string trimmed = parts[i].Trim();
            if (!string.IsNullOrEmpty(trimmed) && trimmed != "?")
            {
                currentSequence.Add(trimmed[0]);
            }
        }

        correctAnswer = currentPattern.correctAnswer;

        List<char> answers = new List<char> { correctAnswer };
        answers.AddRange(currentPattern.wrongAnswers);
        answers = answers.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < answerButtons.Count && i < answers.Count; i++)
        {
            answerButtons[i].SetAnswer(answers[i]);
        }

        Debug.Log($"[DataAnalyst] Loaded: {currentPattern.name} " +
                  $"(difficulty: {currentPattern.difficulty}, " +
                  $"remaining: {availablePatterns.Count})");
    }

    public void OnAnswerSelected(int buttonIndex)
    {
        if (finished)
        {
            Debug.Log("[DataAnalyst] OnAnswerSelected called but challenge already finished");
            return;
        }

        attempts++;
        bool isCorrect = answerButtons[buttonIndex].symbol == correctAnswer;

        Debug.Log($"[DataAnalyst] Answer: {answerButtons[buttonIndex].symbol}, " +
                  $"correct={isCorrect}, attempts={attempts}/{maxAttempts}");

        if (isCorrect)
        {
            correctAnswers++;
            ShowFeedback(" Правильно! Вы нашли закономерность!", Color.green);

            SetButtonsInteractable(false);
            Invoke(nameof(ContinueAfterCorrect), 1.5f);
        }
        else
        {
            ShowFeedback("Неправильно! Подумайте ещё.", Color.red);
            StartCoroutine(FlashWrongButton(buttonIndex));
        }

        UpdateUI();
        CheckCompletion();
    }

    private System.Collections.IEnumerator FlashWrongButton(int index)
    {
        var button = answerButtons[index].button;
        var colors = button.colors;
        Color originalColor = colors.normalColor;

        colors.normalColor = Color.red;
        button.colors = colors;

        yield return new WaitForSeconds(0.5f);

        colors.normalColor = originalColor;
        button.colors = colors;
    }

    private void ContinueAfterCorrect()
    {
        SetButtonsInteractable(true);

        if (correctAnswers >= requiredCorrect)
        {
            Debug.Log("[DataAnalyst] Required correct answers reached!");
            EndChallenge(true, "success");
        }
        else
        {
            LoadNewPattern();
            UpdateUI();
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var btn in answerButtons)
        {
            if (btn.button != null)
                btn.button.interactable = interactable;
        }
    }

    private void CheckCompletion()
    {
        if (finished) return;

        if (attempts >= maxAttempts && correctAnswers < requiredCorrect)
        {
            Debug.Log("[DataAnalyst] Max attempts reached without enough correct answers");
            var type = RunContext.Instance.CurrentProfession;

            Debug.Log($"[DataAnalyst] FINAL TYPE SNAPSHOT = {type}");
            EndChallenge(false, "attempts"); 
            return;
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            CancelInvoke(nameof(ClearFeedback));
            feedbackText.text = message;
            feedbackText.color = color;
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
            progressText.text = $"{correctAnswers}/{requiredCorrect}";
        }

        if (sequenceText != null && currentSequence != null)
        {
            sequenceText.text = string.Join(" → ", currentSequence) + " → ?";
        }

        if (questionText != null)
        {
            string difficultyLabel = currentDifficultyLevel switch
            {
                1 => "легко",
                2 => "средне",
                3 => "тяжело",
                _ => ""
            };
            questionText.text = $"Какой символ должен быть следующим?";
        }

        if (difficultyText != null)
        {
            difficultyText.text = $"Сложность: {currentDifficultyLevel}/3";
        }
    }

    private bool ended;

    private void EndChallenge(bool success, string reason = "")
    {
        if (ended) return;
        ended = true;

        finished = true;
        SetButtonsInteractable(false);

        Debug.Log($"[DataAnalyst] END → success={success}, reason={reason}");

        ChallengeManager.Instance?.FinishChallenge(success);

        StartCoroutine(ShowResultDelayed(success, reason));
    }

    private System.Collections.IEnumerator ShowResultDelayed(bool success, string reason)
    {
        yield return new WaitForSeconds(0.5f);

        if (resultWindow != null)
        {
            if (success)
            {
                resultWindow.ShowSuccess(correctAnswers, requiredCorrect, timeLeft);
            }
            else
            {
                resultWindow.ShowFailure(correctAnswers, requiredCorrect, reason);
            }
        }
        else
        {
            Debug.LogWarning("[DataAnalyst] ResultWindow not found!");
        }
    }
}