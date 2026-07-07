using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerClickHandler
{
    public enum ButtonSound
    {
        Default,
        Secondary
    }

    [SerializeField]
    private ButtonSound sound = ButtonSound.Default;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SFXManager.Instance == null)
            return;

        switch (sound)
        {
            case ButtonSound.Default:
                SFXManager.Instance.PlayButton();
                break;

            case ButtonSound.Secondary:
                SFXManager.Instance.PlayButton2();
                break;
        }
    }
}