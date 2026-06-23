using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Settings")]
    public float checkRadius = 200f;
    public float returnDuration = 0.5f;
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual")]
    public Color dragColor = new Color(1, 1, 1, 0.8f);
    public Color normalColor = Color.white;

    private Canvas canvas;
    private RectTransform rect;
    private Vector3 startPos;
    private Transform originalParent;
    private bool isPlaced = false;
    private bool isDragging = false;
    private DropZoneIT nearestZone;
    private DropZoneIT currentZone;
    private Image blockImage;
    private CanvasGroup canvasGroup;
    private Coroutine returnCoroutine;
    private Canvas parentCanvas;
    [Header("Task Content")]
    public TMP_Text label;          // текст строки кода — назначь в инспекторе
    public string UniqueId { get; private set; }
    public bool IsCorrectAnswer { get; private set; }
    [Header("Double Tap")]
    public float doubleTapMaxDelay = 0.35f;

    private float lastTapTime = -999f;
    private int originalSiblingIndex;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        startPos = rect.position;

        // ДОБАВЬ: запоминаем порядковую позицию в Grid Layout

        blockImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;

        foreach (var g in GetComponentsInChildren<Graphic>())
            if (g.gameObject != this.gameObject)
                g.raycastTarget = false;

        Debug.Log($"[CodeBlock] Awake - startPos: {startPos}, originalParent: {originalParent.name}, siblingIndex: {originalSiblingIndex}");
    }
    public void SetContent(string text, string uniqueId, bool isCorrect)
    {
        if (label != null)
            label.text = text;
        originalSiblingIndex = transform.GetSiblingIndex();

        UniqueId = uniqueId;
        IsCorrectAnswer = isCorrect;

        // Если блок ещё лежал в зоне с прошлой попытки — выталкиваем в палитру
        if (isPlaced)
        {
            ExtractFromZone();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlaced || isDragging) return;

        float now = Time.unscaledTime;
        float delta = now - lastTapTime;

        if (delta <= doubleTapMaxDelay)
        {
            Debug.Log("[CodeBlock] Double tap - extracting from zone");
            lastTapTime = -999f;
            ExtractFromZone();
        }
        else
        {
            lastTapTime = now;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[CodeBlock] OnBeginDrag - isPlaced: {isPlaced}, currentZone: {(currentZone != null ? currentZone.name : "null")}");

        if (isDragging) return;

        // Если блок уже размещен - сначала извлекаем его
        if (isPlaced && currentZone != null)
        {
            Debug.Log("[CodeBlock] Extracting block from zone before drag");

            // Сохраняем оригинальную позицию ДО извлечения
            Vector3 originalWorldPos = rect.position;

            // Извлекаем из зоны
            ExtractFromZone();

            // Перемещаем блок под курсор
            isDragging = true;

            // Останавливаем анимацию возврата если есть
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }

            // Перемещаем в корень Canvas для свободного перемещения
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();

            // Устанавливаем позицию под курсором
            rect.position = originalWorldPos;

            // Визуальная обратная связь
            if (blockImage != null)
                blockImage.color = dragColor;

            Debug.Log("[CodeBlock] Block extracted and ready to drag");
            return;
        }

        // Обычное начало перетаскивания
        isDragging = true;

        // Останавливаем анимацию возврата если есть
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        startPos = rect.position;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        // Визуальная обратная связь
        if (blockImage != null)
            blockImage.color = dragColor;

        Debug.Log("[CodeBlock] Begin drag from original position");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        rect.position += (Vector3)eventData.delta / canvas.scaleFactor;

        FindNearestDropZone();
        UpdateDropZones();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[CodeBlock] OnEndDrag - isDragging: {isDragging}");

        if (!isDragging) return;

        isDragging = false;
        ClearAllDropZones();

        // Возвращаем цвет
        if (blockImage != null)
            blockImage.color = normalColor;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        DropZoneIT zone = null;
        foreach (var result in results)
        {
            zone = result.gameObject.GetComponentInParent<DropZoneIT>();
            if (zone != null && !zone.IsOccupied())
            {
                Debug.Log($"[CodeBlock] Found available zone via raycast: {zone.name}");
                break;
            }
            else if (zone != null)
            {
                Debug.Log($"[CodeBlock] Zone found but occupied: {zone.name}");
                zone = null;
            }
        }

        // Проверяем ближайшую зону если рейкаст не нашел
        if (zone == null && nearestZone != null && !nearestZone.IsOccupied())
        {
            float distance = Vector2.Distance(transform.position, nearestZone.transform.position);
            Debug.Log($"[CodeBlock] Nearest zone: {nearestZone.name}, distance: {distance}");

            if (distance < nearestZone.magnetRange * 0.5f)
            {
                zone = nearestZone;
                Debug.Log($"[CodeBlock] Using nearest zone: {zone.name}");
            }
        }

        if (zone == null)
        {
            Debug.Log($"[CodeBlock] No zone found, returning to original position: {startPos}");
            ReturnToOriginalPosition();
        }
        else
        {
            Debug.Log($"[CodeBlock] Placing in zone: {zone.name}");
            // Раньше: zone.OnDrop(eventData) — вызывало гонку с авто-IDropHandler
            // Теперь вызываем AcceptBlock напрямую, без посредника
            zone.AcceptBlock(this);
        }

        nearestZone = null;
    }

    // В ExtractFromZone() сразу после SetParent добавь восстановление позиции:
    private void ExtractFromZone()
    {
        Debug.Log($"[CodeBlock] ExtractFromZone - currentZone: {(currentZone != null ? currentZone.name : "null")}");

        if (currentZone != null)
        {
            currentZone.RemoveBlock(gameObject);
            currentZone = null;
        }

        isPlaced = false;
        isDragging = false;

        transform.SetParent(originalParent, true);

        // ДОБАВЬ: возвращаем блок на его исходное место в сетке,
        // а не в конец — иначе GridLayoutGroup поставит его не туда
        int clampedIndex = Mathf.Min(originalSiblingIndex, originalParent.childCount - 1);
        transform.SetSiblingIndex(clampedIndex);

        if (blockImage != null)
            blockImage.color = normalColor;

        StartCoroutine(SmoothReturnToStart());

        canvasGroup.blocksRaycasts = true;

        var controller = FindObjectOfType<ChallengeITController>();
        if (controller != null)
            controller.OnBlockExtracted();

        Debug.Log("[CodeBlock] Extraction complete");
    }

    private IEnumerator SmoothReturnToStart()
    {
        Vector3 currentPos = rect.position;
        float elapsed = 0f;

        Debug.Log($"[CodeBlock] Smooth return from {currentPos} to {startPos}");

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            t = Mathf.SmoothStep(0, 1, t);

            rect.position = Vector3.Lerp(currentPos, startPos, t);
            yield return null;
        }

        rect.position = startPos;
        Debug.Log($"[CodeBlock] Returned to start position: {startPos}");
    }

    private void ReturnToOriginalPosition()
    {
        Debug.Log($"[CodeBlock] ReturnToOriginalPosition - current pos: {rect.position}, target: {startPos}");

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        returnCoroutine = StartCoroutine(AnimateReturn());
    }

    // В AnimateReturn() — та же логика, перед стартом анимации сразу
    // фиксируем правильную позицию в иерархии:
    private IEnumerator AnimateReturn()
    {
        // ДОБАВЬ перед анимацией: сразу восстанавливаем место в Grid
        transform.SetParent(originalParent, true);
        int clampedIndex = Mathf.Min(originalSiblingIndex, originalParent.childCount - 1);
        transform.SetSiblingIndex(clampedIndex);

        Vector3 currentPos = rect.position;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            t = returnCurve.Evaluate(t);

            rect.position = Vector3.Lerp(currentPos, startPos, t);
            yield return null;
        }

        rect.position = startPos;
        // SetParent здесь больше не нужен — мы это сделали в начале корутины

        returnCoroutine = null;

        Debug.Log($"[CodeBlock] Return animation complete, position: {rect.position}");
    }

    private void FindNearestDropZone()
    {
        DropZoneIT[] allZones = FindObjectsOfType<DropZoneIT>();
        float minDistance = float.MaxValue;
        DropZoneIT closest = null;

        foreach (var zone in allZones)
        {
            if (zone.IsOccupied()) continue;

            float distance = Vector2.Distance(transform.position, zone.transform.position);
            if (distance < minDistance && distance < checkRadius)
            {
                minDistance = distance;
                closest = zone;
            }
        }

        nearestZone = closest;
    }

    private void UpdateDropZones()
    {
        DropZoneIT[] allZones = FindObjectsOfType<DropZoneIT>();

        foreach (var zone in allZones)
        {
            if (zone.IsOccupied()) continue;

            float distance = Vector2.Distance(transform.position, zone.transform.position);
            if (distance < zone.magnetRange)
            {
                zone.OnBlockDragEnter(this);
            }
            else
            {
                zone.OnBlockDragExit();
            }
        }
    }

    private void ClearAllDropZones()
    {
        DropZoneIT[] allZones = FindObjectsOfType<DropZoneIT>();
        foreach (var zone in allZones)
        {
            zone.OnBlockDragExit();
        }
    }

    public void SetPlaced(bool placed)
    {
        isPlaced = placed;
        Debug.Log($"[CodeBlock] SetPlaced: {placed}");
    }

    public void SetCurrentZone(DropZoneIT zone)
    {
        currentZone = zone;
        Debug.Log($"[CodeBlock] SetCurrentZone: {(zone != null ? zone.name : "null")}");
    }

    public void SetOriginalPosition(Vector3 position)
    {
        startPos = position;
        Debug.Log($"[CodeBlock] SetOriginalPosition: {position}");
    }

    private void OnDestroy()
    {
        ClearAllDropZones();
    }
}