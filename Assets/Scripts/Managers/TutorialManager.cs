using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Слайды — назначь в префабе")]
    public GameObject[] slides;

    [Header("Кнопки")]
    public Button nextButton;
    public Button prevButton;
    public Button skipButton;

    [Header("Индикатор страницы")]
    public TextMeshProUGUI pageText;

    private int currentSlide = 0;
    private bool isCompact = false;
    enum TutorialState
    {
        NotShown,
        Showing,
        Shown
    }
    private void Awake()
    {
        if (Instance != null)
        {
            // Уже есть живой или мёртвый Instance — уничтожаем себя
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        ShowSlide(0);
    }

    // Вызывается из GameManager
    public void Init(bool compact)
    {
        isCompact = compact;
        currentSlide = 0;

        if (skipButton != null)
            skipButton.gameObject.SetActive(!compact);

        ShowSlide(0);
    }

    private void ShowSlide(int index)
    {
        if (slides == null || slides.Length == 0) return;

        index = Mathf.Clamp(index, 0, slides.Length - 1);
        currentSlide = index;

        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
                slides[i].SetActive(i == currentSlide);
        }

        UpdatePageText();
        UpdateNextButtonText();
        UpdatePrevButton();
    }

    private void UpdatePageText()
    {
        if (pageText != null)
            pageText.text = $"{currentSlide + 1} / {slides.Length}";
    }

    private void UpdateNextButtonText()
    {
        if (nextButton == null) return;
        var txt = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt == null) return;
        bool isLast = currentSlide >= slides.Length - 1;
        txt.text = isLast ? "Начать!" : "Далее >";
    }

    private void UpdatePrevButton()
    {
        if (prevButton != null)
            prevButton.gameObject.SetActive(currentSlide > 0);
    }

    public void OnNextClicked()
    {
        if (currentSlide >= slides.Length - 1)
        {
            CloseTutorial();
            return;
        }
        ShowSlide(currentSlide + 1);
    }

    public void OnPrevClicked()
    {
        if (currentSlide > 0)
            ShowSlide(currentSlide - 1);
    }

    public void OnSkipClicked()
    {
        CloseTutorial();
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void CloseTutorial()
    {
        SaveSystem.SetTutorialShown();

        if (GameManager.Instance != null)
            GameManager.Instance.OnTutorialClosed();

        Destroy(gameObject);
    }
}