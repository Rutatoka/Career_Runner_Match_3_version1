using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneDesign : MonoBehaviour, IPointerClickHandler
{
    public string correctColorId;
    public Image zoneImage;
    public Image blockedImage;
    public GameObject highlightEffect;

    [Header("Zone Colors")]
    public Color emptyColor = Color.white;
    public Color occupiedColor = new Color(0.8f, 0.8f, 0.8f);
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private DraggableColorBlock current;
    private RectTransform rect;
    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    [Header("Animation")]
    public float pulseDuration = 0.5f;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 5f;
    public float returnDuration = 0.4f; // Длительность анимации возврата

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;

        if (zoneImage == null)
            zoneImage = GetComponent<Image>();

        if (zoneImage != null)
        {
            zoneImage.color = emptyColor;
            zoneImage.raycastTarget = true;
        }

        UpdateVisual();
    }

    public bool IsOccupied() => current != null;

    public bool HasCorrectBlock()
    {
        return current != null && current.colorId == correctColorId;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && current != null)
        {
            Debug.Log("Двойной клик по зоне!");
            ReturnBlockToPalette();
        }
    }

    public void ReturnBlockToPalette()
    {
        if (current != null)
        {
            Debug.Log($"Возвращаем блок {current.colorId} в палитру");

            DraggableColorBlock blockToReturn = current;

            // Останавливаем все анимации
            StopAllCoroutines();

            // СНАЧАЛА очищаем current
            current = null;

            // Обновляем визуал зоны
            UpdateVisual();

            // Возвращаем видимость блоку
            CanvasGroup blockGroup = blockToReturn.GetComponent<CanvasGroup>();
            if (blockGroup != null)
            {
                blockGroup.alpha = 1;
                blockGroup.blocksRaycasts = true;
            }

            // Включаем скрипт блока
            blockToReturn.enabled = true;

            // Запускаем плавную анимацию возврата
            StartCoroutine(AnimateBlockReturn(blockToReturn));

            // Сообщаем контроллеру
            ChallengeDesignController controller = FindObjectOfType<ChallengeDesignController>();
            if (controller != null)
            {
                controller.OnBlockRemoved(blockToReturn.colorId);
            }
        }
    }

    // НОВЫЙ МЕТОД: плавная анимация возврата блока
    private IEnumerator AnimateBlockReturn(DraggableColorBlock block)
    {
        RectTransform blockRect = block.GetComponent<RectTransform>();

        if (blockRect == null) yield break;

        // Сохраняем текущую позицию и размер
        Vector3 startPos = blockRect.position;
        Vector2 startSize = blockRect.sizeDelta;

        // Получаем целевую позицию из блока
        Vector3 targetPos = block.GetStartPosition();
        Vector2 targetSize = block.GetOriginalSize();

        // Сначала сбрасываем anchors чтобы блок мог свободно двигаться
        blockRect.anchorMin = new Vector2(0.5f, 0.5f);
        blockRect.anchorMax = new Vector2(0.5f, 0.5f);
        blockRect.pivot = new Vector2(0.5f, 0.5f);

        // Перемещаем в центр зоны для начала анимации
        blockRect.position = startPos;
        blockRect.sizeDelta = startSize;

        float elapsed = 0;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            t = Mathf.SmoothStep(0, 1, t); // Плавное замедление в конце

            // Плавно двигаем и сжимаем
            blockRect.position = Vector3.Lerp(startPos, targetPos, t);
            blockRect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);

            yield return null;
        }

        // Финально применяем ResetBlock для точного позиционирования
        block.ResetBlock();
    }

    public void Clear()
    {
        if (current != null)
        {
            DraggableColorBlock blockToClear = current;

            StopAllCoroutines();

            current = null;

            CanvasGroup blockGroup = blockToClear.GetComponent<CanvasGroup>();
            if (blockGroup != null)
            {
                blockGroup.alpha = 1;
                blockGroup.blocksRaycasts = true;
            }

            blockToClear.enabled = true;
            blockToClear.ResetBlock();
        }

        UpdateVisual();
    }

    public void SetBlock(DraggableColorBlock block)
    {
        if (current != null)
        {
            DraggableColorBlock oldBlock = current;
            current = null;

            CanvasGroup oldGroup = oldBlock.GetComponent<CanvasGroup>();
            if (oldGroup != null)
            {
                oldGroup.alpha = 1;
                oldGroup.blocksRaycasts = true;
            }
            oldBlock.enabled = true;
            oldBlock.ResetBlock();
        }

        current = block;

        CanvasGroup blockGroup = block.GetComponent<CanvasGroup>();
        if (blockGroup != null)
        {
            blockGroup.blocksRaycasts = false;
        }

        StartCoroutine(StretchBlockToZone(block));

        UpdateVisual();
    }

    private IEnumerator StretchBlockToZone(DraggableColorBlock block)
    {
        RectTransform blockRect = block.GetComponent<RectTransform>();
        RectTransform zoneRect = GetComponent<RectTransform>();

        if (blockRect == null || zoneRect == null) yield break;

        blockRect.SetParent(transform);

        Vector2 startSize = blockRect.sizeDelta;
        Vector2 targetSize = zoneRect.rect.size;

        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);

            blockRect.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            yield return null;
        }

        blockRect.anchorMin = Vector2.zero;
        blockRect.anchorMax = Vector2.one;
        blockRect.offsetMin = Vector2.zero;
        blockRect.offsetMax = Vector2.zero;
        blockRect.sizeDelta = Vector2.zero;

        Image blockImage = block.GetComponent<Image>();
        if (blockImage != null)
        {
            blockImage.raycastTarget = false;
        }
    }

    private void UpdateVisual()
    {
        if (current != null)
        {
            if (zoneImage != null)
            {
                zoneImage.color = occupiedColor;
                zoneImage.raycastTarget = true;
            }

            if (blockedImage != null)
                blockedImage.gameObject.SetActive(true);
        }
        else
        {
            if (zoneImage != null)
            {
                zoneImage.color = emptyColor;
                zoneImage.raycastTarget = true;
            }

            if (blockedImage != null)
                blockedImage.gameObject.SetActive(false);
        }
    }

    public void ShowCorrectAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(CorrectAnimationCoroutine());
    }

    public void ShowWrongAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(WrongAnimationCoroutine());
    }

    private IEnumerator CorrectAnimationCoroutine()
    {
        float elapsed = 0;
        float halfDuration = pulseDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            rect.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            rect.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, t);
            yield return null;
        }

        rect.localScale = originalScale;

        if (zoneImage != null)
        {
            zoneImage.color = correctColor;
            yield return new WaitForSeconds(0.3f);
            zoneImage.color = occupiedColor;
        }

        if (highlightEffect != null)
        {
            highlightEffect.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            highlightEffect.SetActive(false);
        }
    }

    private IEnumerator WrongAnimationCoroutine()
    {
        if (zoneImage != null)
        {
            zoneImage.color = wrongColor;
        }

        float elapsed = 0;
        Vector3 startPos = rect.anchoredPosition;
        float intensity = shakeIntensity;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Sin(elapsed * 30f) * intensity;
            rect.anchoredPosition = startPos + new Vector3(x, 0, 0);
            intensity *= 0.95f;
            yield return null;
        }

        rect.anchoredPosition = startPos;
        yield return new WaitForSeconds(0.5f);

        if (zoneImage != null)
        {
            zoneImage.color = emptyColor;
        }
    }
}