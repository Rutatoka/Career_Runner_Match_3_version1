using UnityEngine;

public class ProfessionObject : MonoBehaviour
{
    [Header("Settings")]
    public ProfessionObjectData data;

    private bool isCollected = false; // Флаг для предотвращения повторного подбора
    private Collider objectCollider;

    private void Awake()
    {
        objectCollider = GetComponent<Collider>();

        // Убеждаемся что коллайдер только один и он триггер
        if (objectCollider != null)
        {
            objectCollider.isTrigger = true;
        }

        // Отключаем все дочерние коллайдеры
        DisableChildColliders();
    }
    private void Start()
    {
        if (data != null)
            ApplyColor(data.directionColor);
    }

    private void DisableChildColliders()
    {
        // Отключаем ВСЕ коллайдеры на дочерних объектах
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (var col in childColliders)
        {
            if (col != objectCollider && col.gameObject != gameObject)
            {
                col.enabled = false;
                Debug.LogWarning($"[ProfessionObject] Disabled child collider on {col.gameObject.name}");
            }
        }
    }

    private void OnEnable()
    {
        // Сбрасываем флаг при активации объекта
        isCollected = false;

        // Включаем коллайдер
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем что это игрок
        if (!other.CompareTag("Player")) return;

        // Проверяем что ещё не собрано
        if (isCollected)
        {
            Debug.LogWarning($"[ProfessionObject] {data?.professionType} already collected, ignoring trigger");
            return;
        }

    //    Debug.Log($"TRIGGER from {other.name} on {gameObject.name}");

        // Сразу отключаем коллайдер чтобы предотвратить повторные вызовы
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        HandlePickup();
        //if (!other.CompareTag("Player")) return;
        //if (isCollected) return;

        //// если игрок уже выбрал профессию — игнорируем другие
        //if (ProfessionSystem.Instance.CurrentProfession != ProfessionType.None &&
        //    ProfessionSystem.Instance.CurrentProfession != data.professionType)
        //{
        //    Debug.Log($"Игнорируем {data.professionType}, игрок уже выбрал {ProfessionSystem.Instance.CurrentProfession}");
        //    return;
        //}

        //isCollected = true;
        //HandlePickup();
    }

    private void HandlePickup()
    {
        if (isCollected) return;

        isCollected = true;

   //   Debug.Log($"HANDLE PICKUP: {data?.professionType}");
        PreferenceAnalyzer.Instance?.
        RegisterObjectPickup(data.professionType);
        if (data == null)
        {
            Debug.LogError("[ProfessionObject] Data is null!");
            return;
        }

        // Отключаем все коллайдеры
        DisableAllColliders();

        // Вызываем сбор через ProfessionSystem
        var system = FindObjectOfType<ProfessionSystem>();
        if (system != null)
        {
            system.CollectProfessionObject(data);
        }
        else
        {
            Debug.LogError("[ProfessionObject] ProfessionSystem not found!");
        }

        // Анимация подбора (опционально)
        StartCoroutine(PickupAnimation());
    }

    private System.Collections.IEnumerator PickupAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Деактивируем объект
        gameObject.SetActive(false);
    }

    private void DisableAllColliders()
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }
    }
  


    public void ApplyColor(Color c)
    {
        // 3D объект
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = c;
            return;
        }

        // UI объект
        var img = GetComponentInChildren<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = c;
            return;
        }

        Debug.LogWarning("[ProfessionObject] No Renderer or Image found to apply color");
    }




    // Публичный метод для ручного сброса
    public void ResetObject()
    {
        isCollected = false;

        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }
}