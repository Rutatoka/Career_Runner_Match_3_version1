using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DraggableColorBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string colorId;
    public Color blockColor;
    public Image blockImage;
    private Vector2 originalSize; // Для хранения исходного размера
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;
    private Vector3 startPos;
    private Transform originalParent;
    private Vector3 targetScale;

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float animationSpeed = 8f;
    public float dragAlpha = 0.7f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        originalParent = transform.parent;
        targetScale = Vector3.one;
        originalSize = rect.sizeDelta; // Сохраняем исходный размер

        if (blockImage == null)
            blockImage = GetComponent<Image>();
    }
    private void Start()
    {
        // Устанавливаем цвет здесь, после всех Awake
        if (blockImage != null && blockColor != Color.clear)
        {
            Color fixedColor = blockColor;
            fixedColor.a = 1f;
            blockImage.color = fixedColor;

            Debug.Log($"Start: Применен цвет {fixedColor} к блоку {colorId}");
        }
    }
    private void Update()
    {
        // Плавное изменение масштаба (НЕ ЦВЕТА!)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rect.position;
        group.alpha = dragAlpha;
        group.blocksRaycasts = false;

        // Поднимаем блок выше других при перетаскивании
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        targetScale = Vector3.one * 1.05f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.alpha = 1f;
        group.blocksRaycasts = true;
        targetScale = Vector3.one;

        DropZoneDesign zone = null;

        // Проверяем, над какой зоной мы находимся
        if (eventData.pointerEnter != null)
        {
            zone = eventData.pointerEnter.GetComponent<DropZoneDesign>();
            if (zone == null)
            {
                zone = eventData.pointerEnter.GetComponentInParent<DropZoneDesign>();
            }
        }

        if (zone != null && !zone.IsOccupied())
        {
            // Успешно поместили в зону
            zone.SetBlock(this);

            // Плавное перемещение в центр зоны
            StartCoroutine(MoveToPosition(zone.transform.position, 0.3f));

            bool correct = (zone.correctColorId == colorId);

            // Небольшая задержка перед проверкой
            StartCoroutine(DelayedCheck(zone, correct));
        }
        else
        {
            // Возвращаем на место
            StartCoroutine(MoveToPosition(startPos, 0.3f));
            transform.SetParent(originalParent);
        }
    }

    private IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        float elapsed = 0;
        Vector3 start = rect.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Добавляем easing (плавное замедление в конце)
            t = Mathf.SmoothStep(0, 1, t);

            rect.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        rect.position = target;
    }

    private IEnumerator DelayedCheck(DropZoneDesign zone, bool correct)
    {
        yield return new WaitForSeconds(0.3f);

        FindObjectOfType<ChallengeDesignController>().OnColorDropped(correct);

        // Визуальная обратная связь
        if (correct)
        {
            zone.ShowCorrectAnimation();
        }
        else
        {
            zone.ShowWrongAnimation();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (group.blocksRaycasts)
        {
            targetScale = Vector3.one * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (group.blocksRaycasts)
        {
            targetScale = Vector3.one;
        }
    }
    public Vector3 GetStartPosition()
    {
        return startPos;
    }

    public Vector2 GetOriginalSize()
    {
        return originalSize;
    }
    public void ResetBlock()
    {
        StopAllCoroutines();

        rect.position = startPos;
        rect.sizeDelta = originalSize;

        transform.SetParent(originalParent);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        group.alpha = 1f;
        group.blocksRaycasts = true;

        targetScale = Vector3.one;
        transform.localScale = Vector3.one;

        enabled = true;

        if (blockImage != null)
        {
            blockImage.enabled = true;
            blockImage.raycastTarget = true;
        }
    }
}