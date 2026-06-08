// FallingIcon.cs
using UnityEngine;

public class FallingIcon : MonoBehaviour
{
    public float speed = 400f;
    public bool isGood = true;

    private RectTransform rect;
    private RectTransform playerRect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Находим игрока по тегу
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerRect = player.GetComponent<RectTransform>();
    }

    private void Update()
    {
        rect.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        // Проверяем пересечение с игроком
        if (playerRect != null && RectTransformOverlaps(rect, playerRect))
        {
            CatchIcon();
            return;
        }

        if (rect.anchoredPosition.y < -1200f)
            Destroy(gameObject);
    }

    private bool RectTransformOverlaps(RectTransform a, RectTransform b)
    {
        // Получаем мировые координаты углов
        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];

        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);

        // Проверяем пересечение прямоугольников
        return !(aCorners[1].x > bCorners[3].x ||
                 aCorners[3].x < bCorners[1].x ||
                 aCorners[1].y < bCorners[3].y ||
                 aCorners[3].y > bCorners[1].y);
    }

    private void CatchIcon()
    {
        var controller = FindObjectOfType<ChallengeMarketingController>();

        if (isGood)
            controller.CatchGood();
        else
            controller.CatchBad();

        Destroy(gameObject);
    }
}