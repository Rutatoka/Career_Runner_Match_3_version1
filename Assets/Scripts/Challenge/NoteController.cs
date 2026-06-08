using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class NoteController : MonoBehaviour, IPointerDownHandler
{
    [Header("Fade out")]
    public float destroyDelay = 0.2f;
    public float fadeDuration = 0.1f;

    private ChallengeMediaController controller;
    private int lane;
    private Transform target;
    private float speed;
    private float perfectThreshold;
    private float goodThreshold;
    private bool isHit = false;
    private bool isMissed = false;
    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(ChallengeMediaController gameController, int laneIndex, Transform targetPoint, float noteSpeed, float perfectThresh, float goodThresh)
    {
        controller = gameController;
        lane = laneIndex;
        target = targetPoint;
        speed = noteSpeed;
        perfectThreshold = perfectThresh;
        goodThreshold = goodThresh;
    }

    private void Update()
    {
        if (isHit || isMissed) return;
        rect.Translate(Vector3.down * speed * Time.deltaTime);
        if (target != null && rect.position.y < target.position.y - goodThreshold)
            Miss();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isHit && !isMissed) TryHit();
    }

    public void TryHit()
    {
        if (isHit || isMissed || controller == null || target == null) return;
        float distance = Mathf.Abs(rect.position.y - target.position.y);
        controller.OnNoteHit(lane, distance);
        isHit = true;
        StartCoroutine(FadeOut());
    }

    private void Miss()
    {
        if (isMissed || isHit) return;
        isMissed = true;
        controller?.OnNoteMiss(lane);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        yield return new WaitForSeconds(destroyDelay);
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        Destroy(gameObject);
    }

    public bool CanBeHit(float maxDistance)
    {
        if (isHit || isMissed || target == null) return false;
        return Mathf.Abs(rect.position.y - target.position.y) <= maxDistance;
    }

    public float GetDistanceToTarget()
    {
        return target == null ? float.MaxValue : Mathf.Abs(rect.position.y - target.position.y);
    }
}