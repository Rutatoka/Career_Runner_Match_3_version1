using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterData
{
    public string characterName = "Игрок";
    public int gender = 0;
    public Color torsoColor = Color.white;
    public Color pantsColor = Color.white;
    public Color shoesColor = Color.white;
    public string equippedOutfit = "";
    public string equippedAccessory = "";
    public string equippedAppearance = "";
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerStats PlayerStats { get; private set; }

    public List<ProfessionData> professions;
    public ProfessionData.ProfessionCategory CategoryResult { get; private set; }
    public ProfessionData DirectionResult { get; private set; }
    public bool HasCategoryResult { get; private set; }

    public enum GameState { Bootstrap, Menu,MyPath, Game, Result, Shop, DailyTasks, Profile, MiniGames, Settings }
    public GameState State { get; private set; }
    public bool IsPaused { get; private set; }

    public CharacterData characterData = new CharacterData();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load persistent profile via SaveSystem
        characterData = SaveSystem.GetCharacter();
        PlayerStats = new PlayerStats();
        State = GameState.Bootstrap;
    }

    private void Start()
    {
        GoToMenu();
    }

    public void GoToMenu()
    {
        State = GameState.Menu;
        SceneManager.LoadScene("MainMenu");
        HeaderFooterManager.Instance?.UpdateUI();
    }
    public void GoToMyPath()
    {
        State = GameState.MyPath;
        SceneManager.LoadScene("MyPath");
     
    }

    public void GoToShop()
    {
        State = GameState.Shop;
        SceneManager.LoadScene("ShopScene");
        HeaderFooterManager.Instance?.UpdateUI();
    }

    public void GoToDailyTasks()
    {
        State = GameState.DailyTasks;
        SceneManager.LoadScene("DailyTasksScene");
        HeaderFooterManager.Instance?.UpdateUI();
    }

    public void GoToProfile()
    {
        State = GameState.Profile;
        SceneManager.LoadScene("ProfileScene");
        HeaderFooterManager.Instance?.UpdateUI();
    }

    public void GoToMiniGames()
    {
        State = GameState.MiniGames;
        SceneManager.LoadScene("MiniGamesScene");
        HeaderFooterManager.Instance?.UpdateUI();
    }

    public void GoToSettings()
    {
        State = GameState.Settings;
        SceneManager.LoadScene("SettingsScene");
        HeaderFooterManager.Instance?.UpdateUI();
    }

    public void StartGame()
    {
        PlayerStats = new PlayerStats();
        SetPaused(false);
        CategoryResult = default;
        DirectionResult = null;
        HasCategoryResult = false;
        State = GameState.Game;
        SceneManager.LoadScene("Game");
        HeaderFooterManager.Instance?.UpdateUI();
        var spawner = FindObjectOfType<ProfessionSpawner>();
        if (spawner != null)
            spawner.ResetSpawnStats();
        FindObjectOfType<TileObjectSpawner>()?.ResetGlobalZ();

        ProfessionSystem.Instance?.ResetProgress();

        Invoke(nameof(StartRunnerAfterLoad), 0.1f);
        Invoke(nameof(UpdateGameCharacter), 0.1f);
    }
    private void StartRunnerAfterLoad()
    {
        RunnerController.Instance?.StartRun();
    }

    private void UpdateGameCharacter()
    {
        var model = FindObjectOfType<CharacterModelController>();
        model?.UpdateCharacter();
    }

    // Currency via SaveSystem
    public int GetGems() => SaveSystem.GetGems();
    public void AddGems(int amount)
    {
        SaveSystem.AddGems(amount);
        HeaderFooterManager.Instance?.Refresh();
    }

    public bool SpendGems(int amount) => SaveSystem.SpendGems(amount);

    public void AddCoins(int amount)
    {
        SaveSystem.AddCoins(amount);
        HeaderFooterManager.Instance?.Refresh();
    }

    public bool SpendCoins(int amount) => SaveSystem.SpendCoins(amount);

    // Daily tasks compatibility (legacy methods kept)
    public void UpdateDailyTaskProgress(string taskTitle, int amount)
    {
        // Forward to DailyTasksManager if present
        var dtm = FindObjectOfType<DailyTasksManager>();
        if (dtm != null)
        {
            dtm.AddProgressToTask(taskTitle, amount);
            return;
        }

        // Fallback: update PlayerPrefs directly (legacy)
        string progressKey = $"daily_{DailyTask.MakeSafeId(taskTitle)}_progress";
        int current = PlayerPrefs.GetInt(progressKey, 0) + amount;
        PlayerPrefs.SetInt(progressKey, current);
        PlayerPrefs.Save();
    }

    public void CompleteDailyTask(string taskTitle)
    {
        var dtm = FindObjectOfType<DailyTasksManager>();
        if (dtm != null)
        {
            dtm.AddProgressToTask(taskTitle, int.MaxValue);
            return;
        }
        string key = $"daily_{DailyTask.MakeSafeId(taskTitle)}_completed";
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public bool IsDailyTaskCompleted(string taskTitle)
    {
        var dtm = FindObjectOfType<DailyTasksManager>();
        if (dtm != null)
        {
            var task = dtm != null ? dtm.GetType().GetField("tasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(dtm) : null;
            // fallback to PlayerPrefs
        }
        string key = $"daily_{DailyTask.MakeSafeId(taskTitle)}_completed";
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public int GetDailyTaskProgress(string taskTitle)
    {
        var dtm = FindObjectOfType<DailyTasksManager>();
        if (dtm != null)
        {
            // prefer manager
        }
        string progressKey = $"daily_{DailyTask.MakeSafeId(taskTitle)}_progress";
        return PlayerPrefs.GetInt(progressKey, 0);
    }

    public void SetPaused(bool value)
    {
        IsPaused = value;
        Time.timeScale = value ? 0f : 1f;
    }

    public void SetCategoryResult(ProfessionData.ProfessionCategory result)
    {
        CategoryResult = result;
        HasCategoryResult = true;
    }

    public void SetDirectionResult(ProfessionData result)
    {
        DirectionResult = result;
    }
    public void SaveCharacterData()
    {
        if (characterData == null)
        {
            Debug.LogWarning("GameManager: characterData is NULL, cannot save.");
            return;
        }

        // Сохраняем JSON в PlayerPrefs
        SaveSystem.SetCharacter(characterData);

        Debug.Log("CharacterData saved.");
    }
    //public void LoadCharacterData()
    //{
    //    if (PlayerPrefs.HasKey("CharacterData"))
    //    {
    //        string json = PlayerPrefs.GetString("CharacterData");
    //        characterData = JsonUtility.FromJson<CharacterData>(json);
    //    }
    //    else
    //    {
    //        // Если данных нет — создаём новые
    //        characterData = new CharacterData();
    //        SaveCharacterData();
    //    }
    //}
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public static class ProfessionUtils
    {
        public static string GetCategoryName(ProfessionData.ProfessionCategory cat)
        {
            return cat switch
            {
                ProfessionData.ProfessionCategory.Key => "Ключевые направления",
                ProfessionData.ProfessionCategory.Creative => "Креативные",
                ProfessionData.ProfessionCategory.Social => "Социальные",
                ProfessionData.ProfessionCategory.Business => "Бизнес",
                ProfessionData.ProfessionCategory.Specialization => "Специализации",
                ProfessionData.ProfessionCategory.Additional => "Дополнительные",
                _ => "Неизвестно"
            };
        }
    }
}
