using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    public Button buttonStart;
    public Button buttonExit;
    public Button buttonMyPath;

    public CharacterModelController modelController;

    private void Start()
    {
        UpdateCharacter();
        buttonStart.onClick.AddListener(() => GameManager.Instance.StartGame());
        buttonExit.onClick.AddListener(() => GameManager.Instance.ExitGame());
        buttonMyPath.onClick.AddListener(() => GameManager.Instance.GoToMyPath());

        // Проверяем условия для показа кнопки
        UpdateMyPathButton();
    }

    private void UpdateCharacter()
    {
        if (GameManager.Instance != null)
        {
            if (modelController != null)
                modelController.UpdateCharacter();
        }
    }

    private void OnEnable()
    {
        UpdateCharacter();
        // Обновляем состояние кнопки при каждом показе меню
        UpdateMyPathButton();
    }

    private void UpdateMyPathButton()
    {
        if (buttonMyPath == null)
        {
            Debug.LogWarning("[MenuUIController] buttonMyPath is not assigned!");
            return;
        }

        bool shouldShow = false;

        if (PreferenceAnalyzer.Instance != null)
        {
            int totalRuns = PreferenceAnalyzer.Instance.GetTotalRuns();
            bool canRecommend = PreferenceAnalyzer.Instance.CanShowRecommendation();

            shouldShow = totalRuns >= PreferenceAnalyzer.Instance.minRunsForRecommendation;

            Debug.Log($"[MenuUIController] MyPath button: Total runs={totalRuns}, " +
                      $"Min needed={PreferenceAnalyzer.Instance.minRunsForRecommendation}, " +
                      $"Can recommend={canRecommend}, Showing button={shouldShow}");
        }
        else
        {
            Debug.LogWarning("[MenuUIController] PreferenceAnalyzer.Instance is NULL!");
        }

        buttonMyPath.gameObject.SetActive(shouldShow);
    }
}