using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeaderFooterManager : MonoBehaviour
{
    public static HeaderFooterManager Instance { get; private set; }

    [Header("Header")]
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI coinsText;

    [Header("Footer")]
    public Button menuButton;
    public Button dailyTasksButton;
    public Button profileButton;
    public Button shopButton;

    private const float CurrencyPollInterval = 1f;

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
        SafeSetupButtons();
        UpdateUI();
        UpdateCurrency();
        InvokeRepeating(nameof(UpdateCurrency), CurrencyPollInterval, CurrencyPollInterval);
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(UpdateCurrency));
        // Убираем слушатели, чтобы не было утечек
        if (menuButton != null) menuButton.onClick.RemoveAllListeners();
        if (dailyTasksButton != null) dailyTasksButton.onClick.RemoveAllListeners();
        if (profileButton != null) profileButton.onClick.RemoveAllListeners();
        if (shopButton != null) shopButton.onClick.RemoveAllListeners();
        if (Instance == this) Instance = null;
    }

    private void SafeSetupButtons()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("HeaderFooterManager: GameManager.Instance is null. Buttons will be wired later.");
            return;
        }

        // Очистка и назначение слушателей с проверками на null
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => {
                var state = GameManager.Instance.State;
                if (state == GameManager.GameState.Menu)
                    GameManager.Instance.GoToSettings();
                else
                    GameManager.Instance.GoToMenu();
            });
        }

        if (dailyTasksButton != null)
        {
            dailyTasksButton.onClick.RemoveAllListeners();
            dailyTasksButton.onClick.AddListener(() => GameManager.Instance.GoToDailyTasks());
        }

        if (profileButton != null)
        {
            profileButton.onClick.RemoveAllListeners();
            profileButton.onClick.AddListener(() => GameManager.Instance.GoToProfile());
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(() => GameManager.Instance.GoToShop());
        }
    }

    public void UpdateUI()
    {
        if (GameManager.Instance == null)
        {
            gameObject.SetActive(true);
            return;
        }

        // Скрываем в игровом режиме
        if (GameManager.Instance.State == GameManager.GameState.Game)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        UpdateMenuButtonText();
    }

    private void UpdateMenuButtonText()
    {
        if (menuButton == null) return;
        var txt = menuButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt == null) return;

        if (GameManager.Instance == null)
        {
            txt.text = "Главное меню";
            return;
        }

        switch (GameManager.Instance.State)
        {
            case GameManager.GameState.Menu:
                txt.text = "Настройки";
                break;
            case GameManager.GameState.Settings:
                txt.text = "Главное меню";
                break;
            default:
                txt.text = "Главное меню";
                break;
        }
    }

    public void UpdateCurrency()
    {
        // Предпочтительно использовать SaveSystem, а не PlayerPrefs напрямую
        int gems = SaveSystem.GetGems();
        int coins = SaveSystem.GetCoins();

        if (gemsText != null) gemsText.text = gems.ToString();
        if (coinsText != null) coinsText.text = coins.ToString();
    }

    public void Refresh()
    {
        UpdateCurrency();
        UpdateUI();
    }
}
