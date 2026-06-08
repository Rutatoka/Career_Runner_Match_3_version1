using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class HappinessSystem : MonoBehaviour
{
    public static HappinessSystem Instance { get; private set; }

    public event Action<int> OnGemsChanged;

    [Header("UI")]
    public Slider slider;
    public TMP_Text numGem;

    [Header("Settings")]
    public int max = 100;
    public int gems;

    public int CurrentValue { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SaveSystemAvailable())
            CurrentValue = Mathf.Clamp(SaveSystem.GetGems(), 0, max);
        else
            CurrentValue = Mathf.Clamp(PlayerPrefs.GetInt("Gems", 0), 0, max);

        UpdateUI();
    }


    private void TryFindUI()
    {
        if (slider == null)
            slider = FindObjectOfType<Slider>();
       // Debug.Log("FOUND SLIDER: " + slider.name + " in " + slider.transform.parent.name);

        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            numGem = canvas.transform.Find("Happiness/NumGemTxt")?.GetComponent<TMP_Text>();
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (SaveSystemAvailable())
        {
            int delta = CurrentValue - SaveSystem.GetGems();
            if (delta > 0) SaveSystem.AddGems(delta);
            else if (delta < 0)
            {
                int toSpend = -delta;
                SaveSystem.SpendGems(toSpend);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Gems", CurrentValue);
            PlayerPrefs.Save();
        }

        if (Instance == this) Instance = null;
    }

    public void Add(int amount)
    {
        if (amount == 0) return;
        gems = CurrentValue;
        int newValue = Mathf.Clamp(CurrentValue + amount, 0, max);
        if (newValue == CurrentValue) return;

        CurrentValue = newValue;

        if (SaveSystemAvailable())
        {
            int delta = CurrentValue - SaveSystem.GetGems();
            if (delta > 0) SaveSystem.AddGems(delta);
            else if (delta < 0) SaveSystem.SpendGems(-delta);
        }
        else
        {
            PlayerPrefs.SetInt("Gems", CurrentValue);
            PlayerPrefs.Save();
        }

        UpdateUI();
        OnGemsChanged?.Invoke(CurrentValue);
        HeaderFooterManager.Instance?.Refresh();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindUI();
    }
    public void Set(int value)
    {
        value = Mathf.Clamp(value, 0, max);
        int delta = value - CurrentValue;
        if (delta == 0) return;
        Add(delta);
    }

    private void UpdateUI()
    {
        if (numGem != null)
            numGem.text = CurrentValue.ToString();

        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = CurrentValue;
        }
    }


    private bool SaveSystemAvailable()
    {
        try
        {
            var _ = SaveSystem.GetGems();
            return true;
        }
        catch { return false; }
    }
}
