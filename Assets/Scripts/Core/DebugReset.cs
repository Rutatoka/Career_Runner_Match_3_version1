using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DebugReset : MonoBehaviour
{
    public Button resetButton;

    private void Awake()
    {
        if (resetButton == null)
        {
            Debug.LogError("[DebugReset] resetButton is NULL");
            return;
        }

        resetButton.onClick.AddListener(ResetAllPlayerPrefs);
    }

    private void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SaveSystem.ClearSave();
        PreferenceAnalyzer.Instance?.ResetAllStats();
        HappinessSystem.Instance?.Set(0);
        Debug.Log("FULL RESET DONE");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}