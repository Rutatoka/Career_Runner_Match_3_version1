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
    public TMP_Text taskText;

    private int attempts = 0;
    private float timeLeft;
    private bool finished = false;
    [Header("Task Database")]
    public List<ITTaskData> taskDatabase;
    public List<CodeBlock> allBlocks;
    private ITTaskData currentTask;
    [System.Serializable]
    public class ITTaskData
    {
        public string language;          // "Python", "JavaScript", "C#", "Java"
        [TextArea(2, 4)]
        public string taskDescription;   // что нужно собрать
        public string[] correctLines;    // 3 строки — правильный порядок
        public string[] wrongLines;      // 3 строки — отвлекающие варианты
    }
    [ContextMenu("Generate Default IT Tasks")]
    [ContextMenu("Generate Default IT Tasks")]
    public void GenerateDefaultTasks()
    {
        taskDatabase = new List<ITTaskData>
    {
        // ───── Python ─────
        new ITTaskData
        {
            language = "Python",
            taskDescription = "Собери код: вывести числа от 1 до 5",
            correctLines = new[]
            {
                "for i in range(1, 6):",
                "    print(i)",
                "print('Готово')"
            },
            wrongLines = new[]
            {
                "for i in range(10, 1):",
                "    delete(i)",
                "print(undefined)"
            }
        },
        new ITTaskData
        {
            language = "Python",
            taskDescription = "Собери код: проверить чётное число",
            correctLines = new[]
            {
                "number = 8",
                "if number % 2 == 0:",
                "    print('Чётное')"
            },
            wrongLines = new[]
            {
                "number == 8",
                "if number / 2:",
                "    print(number - number)"
            }
        },

        // ───── JavaScript ─────
        new ITTaskData
        {
            language = "JavaScript",
            taskDescription = "Собери код: вывести сообщение в консоль",
            correctLines = new[]
            {
                "let message = 'Привет';",
                "console.log(message);",
                "alert(message);"
            },
            wrongLines = new[]
            {
                "message = console;",
                "log.console(message);",
                "alert == message;"
            }
        },
        new ITTaskData
        {
            language = "JavaScript",
            taskDescription = "Собери код: сложить два числа",
            correctLines = new[]
            {
                "let a = 5;",
                "let b = 10;",
                "let sum = a + b;"
            },
            wrongLines = new[]
            {
                "let a == 5;",
                "var b -> 10;",
                "sum = a / undefined;"
            }
        },

        // ───── C# ─────
        new ITTaskData
        {
            language = "C#",
            taskDescription = "Собери код: вывести приветствие",
            correctLines = new[]
            {
                "string name = \"Игрок\";",
                "Debug.Log(name);",
                "Debug.Log(\"Привет!\");"
            },
            wrongLines = new[]
            {
                "string name == Игрок;",
                "Log.Debug(name);",
                "Debug.Log(name + undefined);"
            }
        },
        new ITTaskData
        {
            language = "C#",
            taskDescription = "Собери код: сравнить два числа",
            correctLines = new[]
            {
                "int a = 7;",
                "int b = 3;",
                "bool result = a > b;"
            },
            wrongLines = new[]
            {
                "int a == 7;",
                "b = int 3;",
                "result == greater(a, b);"
            }
        },

        // ───── Java ─────
        new ITTaskData
        {
            language = "Java",
            taskDescription = "Собери код: вывести число на экран",
            correctLines = new[]
            {
                "int score = 100;",
                "System.out.println(score);",
                "System.out.println(\"Конец\");"
            },
            wrongLines = new[]
            {
                "int score == 100;",
                "println.System(score);",
                "System.out(score, end);"
            }
        },
        new ITTaskData
        {
            language = "Java",
            taskDescription = "Собери код: создать список имён",
            correctLines = new[]
            {
                "String name = \"Аня\";",
                "int age = 15;",
                "System.out.println(name);"
            },
            wrongLines = new[]
            {
                "String name == Аня;",
                "age -> 15;",
                "println(name, System);"
            }
        }
    };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"Generated {taskDatabase.Count} IT tasks");
    }
    private void Start()
    {
        Debug.Log("[ChallengeIT] Start");
        timeLeft = duration;

        if (dropZones == null || dropZones.Count == 0)
            dropZones = new List<DropZoneIT>(FindObjectsOfType<DropZoneIT>());

        if (allBlocks == null || allBlocks.Count == 0)
            allBlocks = new List<CodeBlock>(FindObjectsOfType<CodeBlock>());

        if (resultWindow == null)
            resultWindow = FindObjectOfType<ResultWindow>();

        SetupRandomTask();

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
    private void SetupRandomTask()
    {
        if (taskDatabase == null || taskDatabase.Count == 0)
        {
            Debug.LogError("[ChallengeIT] Task database is empty!");
            return;
        }

        int lastTaskIndex = SaveSystem.GetLastITTaskIndex();

        int index;
        if (taskDatabase.Count == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, taskDatabase.Count);
            } while (index == lastTaskIndex);
        }

        SaveSystem.SetLastITTaskIndex(index);
        currentTask = taskDatabase[index];

        if (taskText != null)
            taskText.text = $"[{currentTask.language}]\n{currentTask.taskDescription}";

        List<(string text, bool isCorrect)> lines = new List<(string, bool)>();
        foreach (var line in currentTask.correctLines)
            lines.Add((line, true));
        foreach (var line in currentTask.wrongLines)
            lines.Add((line, false));

        lines = lines.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < allBlocks.Count && i < lines.Count; i++)
        {
            var block = allBlocks[i];
            string blockId = $"block_{i}";
            block.SetContent(lines[i].text, blockId, lines[i].isCorrect);
        }

        var correctBlocks = allBlocks
            .Where(b => b.IsCorrectAnswer)
            .ToList();

        for (int i = 0; i < dropZones.Count && i < correctBlocks.Count; i++)
        {
            dropZones[i].requiredBlockName = correctBlocks[i].UniqueId;
            dropZones[i].ResetZone();
        }
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