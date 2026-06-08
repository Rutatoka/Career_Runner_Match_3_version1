using UnityEngine;
using UnityEngine.UI;

public enum ColorType
{
    Red,
    Yellow,
    Green
}

public class FolderButton : MonoBehaviour
{
    [Header("Settings")]
    public ColorType colorType;
    public Button button;
    public Image folderImage;

    private ChallengeManagementController controller;

    private void Start()
    {
        controller = FindObjectOfType<ChallengeManagementController>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (controller != null)
            controller.OnFolderClicked(colorType);
    }

    public void SetInteractable(bool state)
    {
        if (button != null)
            button.interactable = state;
    }

    public void ResetButton()
    {
        // Визуальный сброс (если нужно)
    }
}