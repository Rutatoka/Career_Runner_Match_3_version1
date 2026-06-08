using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneIT : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("DropZone Settings")]
    public string requiredBlockName;
    public float magnetRange = 150f;
    public float magnetStrength = 15f;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(0.8f, 0.8f, 0.8f);
    public Color magnetColor = new Color(0.5f, 0.8f, 1f);
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private Image backgroundImage;
    private RectTransform rectTransform;
    private bool isOccupied = false;
    private GameObject placedBlock;
    private CodeBlock currentDraggingBlock;
    private bool isCorrectBlock = false; // Отслеживаем правильность блока

    public bool IsOccupied() => isOccupied;
    public GameObject GetPlacedBlock() => placedBlock;

    // Проверяет, правильный ли блок находится в зоне
    public bool HasCorrectBlock()
    {
        return isOccupied && isCorrectBlock;
    }

    private void Start()
    {
        backgroundImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    private void Update()
    {
        if (!isOccupied && currentDraggingBlock != null)
        {
            float distance = Vector2.Distance(
                currentDraggingBlock.transform.position,
                transform.position
            );

            if (distance < magnetRange)
            {
                Vector2 direction = (transform.position - currentDraggingBlock.transform.position).normalized;
                float force = (1 - distance / magnetRange) * magnetStrength;
                currentDraggingBlock.transform.position += (Vector3)(direction * force * Time.deltaTime);

                if (backgroundImage != null && backgroundImage.color != magnetColor)
                {
                    backgroundImage.color = magnetColor;
                }
            }
            else
            {
                if (backgroundImage != null && backgroundImage.color == magnetColor)
                {
                    backgroundImage.color = highlightColor;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOccupied && backgroundImage != null)
            backgroundImage.color = highlightColor;

        Debug.Log($"[DropZone] POINTER ENTER '{name}'");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOccupied && backgroundImage != null && currentDraggingBlock == null)
            backgroundImage.color = normalColor;

        Debug.Log($"[DropZone] POINTER EXIT '{name}'");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"[DropZone] OnDrop on '{name}'");

        if (eventData.pointerDrag == null) return;
        if (isOccupied)
        {
            Debug.Log($"[DropZone] '{name}' уже занята!");
            return;
        }

        var block = eventData.pointerDrag.GetComponent<CodeBlock>();
        if (block == null) return;

        AcceptBlock(block);
        currentDraggingBlock = null;
    }

    public void AcceptBlock(CodeBlock block)
    {
        Debug.Log($"[DropZone] ACCEPT BLOCK '{block.name}', required: '{requiredBlockName}'");

        // Сохраняем ссылку на зону в блоке
        block.SetCurrentZone(this);

        // Размещаем блок в зоне
        block.transform.SetParent(transform);

        // Анимируем размещение
        StartCoroutine(SnapToCenter(block.transform));

        placedBlock = block.gameObject;

        // Проверяем правильность
        isCorrectBlock = block.name == requiredBlockName;

        // Визуальная обратная связь
        isOccupied = true;
        UpdateZoneVisual();

        // Помечаем блок как размещенный
        block.SetPlaced(true);

        // Оставляем возможность клика для извлечения
        var canvasGroup = block.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        var controller = FindObjectOfType<ChallengeITController>();
        if (controller != null)
            controller.OnBlockDropped(isCorrectBlock);
    }

    private void UpdateZoneVisual()
    {
        if (backgroundImage != null)
        {
            if (!isOccupied)
                backgroundImage.color = normalColor;
            else if (isCorrectBlock)
                backgroundImage.color = correctColor;
            else
                backgroundImage.color = wrongColor;
        }

        // Также обновляем цвет блока
        if (placedBlock != null)
        {
            var blockImage = placedBlock.GetComponent<Image>();
            if (blockImage != null)
            {
                blockImage.color = isCorrectBlock ? correctColor : wrongColor;
            }
        }
    }

    public void RemoveBlock(GameObject block)
    {
        Debug.Log($"[DropZone] RemoveBlock from '{name}', block: {block.name}");

        if (placedBlock == block)
        {
            placedBlock = null;
            isOccupied = false;
            isCorrectBlock = false;

            UpdateZoneVisual();

            Debug.Log($"[DropZone] Block removed, zone is now free");
        }
        else
        {
            Debug.LogWarning($"[DropZone] Trying to remove block that is not placed here!");
        }
    }

    private System.Collections.IEnumerator SnapToCenter(Transform blockTransform)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = blockTransform.localPosition;
        Vector3 targetPos = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);
            blockTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        blockTransform.localPosition = targetPos;
    }

    public void OnBlockDragEnter(CodeBlock block)
    {
        if (!isOccupied)
        {
            currentDraggingBlock = block;
            if (backgroundImage != null)
                backgroundImage.color = magnetColor;
        }
    }

    public void OnBlockDragExit()
    {
        currentDraggingBlock = null;
        if (!isOccupied && backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    public void ResetZone()
    {
        isOccupied = false;
        isCorrectBlock = false;
        if (placedBlock != null)
        {
            Destroy(placedBlock);
            placedBlock = null;
        }
        UpdateZoneVisual();
    }
}