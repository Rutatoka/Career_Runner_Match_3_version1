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

    // Больше не храним собственную копию баланса —
    // SaveSystem всегда единственный источник правды
    public int CurrentValue => SaveSystem.GetGems();

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
        UpdateUI();
    }

    private void TryFindUI()
    {
        if (slider == null)
            slider = FindObjectOfType<Slider>();

        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            numGem = canvas.transform.Find("Happiness/NumGemTxt")?.GetComponent<TMP_Text>();
        }
        UpdateUI();
    }

    // OnDestroy больше ничего не пишет — нет своего состояния, нечего сохранять
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Add(int amount)
    {
     //   Debug.Log($" ADD in hS Attempting to change gems by {amount}. Current gems: {SaveSystem.GetGems()}");
        if (amount == 0) return;

        if (amount > 0)
            SaveSystem.AddGems(amount);
        else
            SaveSystem.SpendGems(-amount);

        // Если UI ещё не найден — пытаемся найти прямо сейчас
        if (numGem == null || slider == null)
            TryFindUI();


        int clamped = Mathf.Clamp(SaveSystem.GetGems(), 0, max);
      //  Debug.Log($"Gems changed by {amount}, new value: {clamped}");
        UpdateUI();
        OnGemsChanged?.Invoke(clamped);
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
        // Ждём один кадр чтобы Canvas в новой сцене точно успел построиться
        StartCoroutine(TryFindUIDelayed());
    }

    private System.Collections.IEnumerator TryFindUIDelayed()
    {
        yield return null;
        yield return null;
        TryFindUI();
    }
    public void Set(int value)
    {
        int current = SaveSystem.GetGems();
        int delta = value - current;
        if (delta == 0) return;
        Add(delta);
    }

    private void UpdateUI()
    {
        int value = Mathf.Clamp(SaveSystem.GetGems(), 0, max);

        if (numGem != null)
            numGem.text = value.ToString();

        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = value;
        }
    }
}