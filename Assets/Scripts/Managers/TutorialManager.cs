using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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

    [Header("Свайп")]
    public float swipeThreshold = 80f;

    [Header("Sorting")]
    [Tooltip("Гарантированно выше HeaderFooterCanvas на время показа туториала")]
    public int forcedSortOrder = 999;

    private int currentSlide = 0;
    private bool isCompact = false;

    private Vector2 touchStartPos;
    private bool isTouching = false;

    // Canvas с тегом UI_Main — поднимаем его поверх HeaderFooterCanvas
    // на время показа туториала, и возвращаем обратно при закрытии
    private Canvas uiMainCanvas;
    private int uiMainOriginalSortOrder;
    private bool uiMainOriginalOverrideSorting;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyForcedSorting();
    }

    private void ApplyForcedSorting()
    {
        uiMainCanvas = FindUIMainCanvas();

        if (uiMainCanvas == null)
        {
            Debug.LogWarning("[TutorialManager] UI_Main canvas not found — sorting not applied");
            return;
        }

        // Запоминаем исходные значения, чтобы вернуть их при закрытии туториала
        uiMainOriginalSortOrder = uiMainCanvas.sortingOrder;
        uiMainOriginalOverrideSorting = uiMainCanvas.overrideSorting;

        uiMainCanvas.overrideSorting = true;
        uiMainCanvas.sortingOrder = forcedSortOrder;
    }

    private void RestoreSorting()
    {
        if (uiMainCanvas == null) return;

        uiMainCanvas.sortingOrder = uiMainOriginalSortOrder;
        uiMainCanvas.overrideSorting = uiMainOriginalOverrideSorting;
    }

    private Canvas FindUIMainCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.CompareTag("UI_Main"))
                return c;
        }
        return null;
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

    public void Init(bool compact)
    {
        isCompact = compact;
        currentSlide = 0;

        if (skipButton != null)
            skipButton.gameObject.SetActive(!compact);

        ShowSlide(0);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        touchStartPos = eventData.position;
        isTouching = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isTouching) return;
        isTouching = false;

        Vector2 delta = eventData.position - touchStartPos;

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            return;

        if (Mathf.Abs(delta.x) < swipeThreshold)
            return;

        if (delta.x < 0)
            OnNextClicked();
        else
            OnPrevClicked();
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
        RestoreSorting();

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