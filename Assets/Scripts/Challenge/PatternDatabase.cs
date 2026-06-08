using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatternDatabase", menuName = "MiniGames/Pattern Database")]
public class PatternDatabase : ScriptableObject
{
    public List<ChallengeDataAnalystController.PatternData> patterns;

    [ContextMenu("Generate Default Patterns")]
    public void GenerateDefaultPatterns()
    {
        patterns = new List<ChallengeDataAnalystController.PatternData>
        {
            // Арифметическая прогрессия (шаг +2)
            new ChallengeDataAnalystController.PatternData
            {
                name = "Арифметическая 1",
                description = "Найдите закономерность в последовательности букв",
                type = ChallengeDataAnalystController.PatternType.Arithmetic,
                difficulty = 1,
                sequenceExample = "A, C, E, G, ?",
                correctAnswer = 'I',
                wrongAnswers = new List<char> { 'H', 'J', 'K' }
            },
            
            // Геометрическая прогрессия
            new ChallengeDataAnalystController.PatternData
            {
                name = "Геометрическая 1",
                description = "Расстояние между буквами увеличивается",
                type = ChallengeDataAnalystController.PatternType.Geometric,
                difficulty = 2,
                sequenceExample = "A, B, D, G, ?",
                correctAnswer = 'K',
                wrongAnswers = new List<char> { 'H', 'L', 'M' }
            },
            
            // Чередование
            new ChallengeDataAnalystController.PatternData
            {
                name = "Чередование 1",
                description = "Буквы берутся с разных концов алфавита",
                type = ChallengeDataAnalystController.PatternType.Alternating,
                difficulty = 2,
                sequenceExample = "A, Z, B, Y, C, ?",
                correctAnswer = 'X',
                wrongAnswers = new List<char> { 'W', 'D', 'V' }
            },
            
            // Пропуск символов
            new ChallengeDataAnalystController.PatternData
            {
                name = "Пропуск 1",
                description = "Каждый раз пропускается на одну букву больше",
                type = ChallengeDataAnalystController.PatternType.Skip,
                difficulty = 3,
                sequenceExample = "A, C, F, J, ?",
                correctAnswer = 'O',
                wrongAnswers = new List<char> { 'K', 'N', 'P' }
            },
            
            // Числовая последовательность
            new ChallengeDataAnalystController.PatternData
            {
                name = "Числовая 1",
                description = "Найдите закономерность в числах",
                type = ChallengeDataAnalystController.PatternType.Arithmetic,
                difficulty = 1,
                sequenceExample = "2, 4, 6, 8, ?",
                correctAnswer = '0', // 10, но берем первый символ
                wrongAnswers = new List<char> { '1', '9', '7' }
            }
        };
    }
}