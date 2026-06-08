using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CourseScreenController : MonoBehaviour
{
    public static CourseScreenController Instance;

    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI professionNameText;
    public TextMeshProUGUI categoryText;

    [Header("Buttons")]
    public Button learnMoreButton;
    public Button continueButton;
    public Button retryButton;

    private ProfessionData currentProfession;
    private ProfessionType currentType;
    private bool lastSuccess;

    public bool IsClosed { get; private set; } = true;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void ShowResult(ProfessionData profession, ProfessionType type, bool success)
    {
        currentProfession = profession;
        currentType = type;
        lastSuccess = success;
        IsClosed = false;
        var stats = PreferenceAnalyzer.Instance.GetAllStats();

        foreach (var stat in stats)
        {
            Debug.Log(
                $"{stat.type} | " +
                $"Objects={stat.objects} | " +
                $"Portals={stat.portals} | " +
                $"Success={stat.successes} | " +
                $"Fail={stat.failures} | " +
                $"Weight={stat.weight}"
            );
        }
        root.SetActive(true);

        titleText.text = success
            ? "Тебе подходит этот стиль мышления"
            : "Возможно, тебе ближе другое направление";

        professionNameText.text = profession != null ? profession.professionName : type.ToString();
        categoryText.text = profession != null ? profession.category.ToString() : "";

        learnMoreButton.gameObject.SetActive(success);
        continueButton.gameObject.SetActive(success);
        retryButton.gameObject.SetActive(!success);

        learnMoreButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        retryButton.onClick.RemoveAllListeners();

        learnMoreButton.onClick.AddListener(OpenCourseLink);
        continueButton.onClick.AddListener(CloseScreen);
        retryButton.onClick.AddListener(RetryChallenge);
    }

    private void OpenCourseLink()
    {
        if (currentProfession != null && !string.IsNullOrEmpty(currentProfession.courseURL))
            Application.OpenURL(currentProfession.courseURL);
    }

    private void CloseScreen()
    {
        root.SetActive(false);
        IsClosed = true;
    }

    private void RetryChallenge()
    {
        root.SetActive(false);
        IsClosed = true;

        ChallengeManager.Instance?.StartChallenge(currentType);
    }
}
