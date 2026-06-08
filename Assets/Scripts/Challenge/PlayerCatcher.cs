using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCatcher : MonoBehaviour, IDragHandler
{
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition = new Vector2(
            Mathf.Clamp(rect.anchoredPosition.x + eventData.delta.x, -450f, 450f),
            rect.anchoredPosition.y
        );
    }
}
