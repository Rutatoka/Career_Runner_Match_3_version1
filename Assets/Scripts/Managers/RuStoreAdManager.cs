using UnityEngine;
using UnityEngine.Events;

public class RuStoreAdManager : MonoBehaviour
{
    public static RuStoreAdManager Instance;

    // Событие, которое вызовется, когда игрок досмотрит рекламу до конца
    public UnityEvent OnRewardedAdFinished;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Пока реклама не интегрирована, просто подготовим событие
        if (OnRewardedAdFinished == null)
            OnRewardedAdFinished = new UnityEvent();
    }

    // Этот метод ты будешь вызывать, когда нужно показать рекламу
    public void ShowRewardedAd()
    {
        Debug.Log("Попытка показать рекламу...");

        // ЗАГЛУШКА: Так как SDK рекламы RuStore еще не подключен,
        // мы сразу вызываем событие успеха, чтобы не ломать игру.
        // Когда установишь Advertisement SDK, заменишь этот блок на реальный вызов.
        Debug.Log("Реклама (заглушка): якобы показана. Вызываем награду.");
        OnRewardedAdFinished?.Invoke();
    }
}