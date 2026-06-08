using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет активными усилителями:
/// - включает эффект
/// - отсчитывает таймер
/// - отключает эффект
/// - уведомляет UI
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("References")]
    public PlayerController player;

    [Header("Magnet Settings")]
    public float magnetForce = 25f;

    // Активные усилители
    private readonly Dictionary<PowerUpType, Coroutine> activeCoroutines = new();
    private readonly Dictionary<PowerUpType, PowerUpItem> activeItems = new();

    // События для UI
    public event Action<PowerUpItem> OnPowerUpActivated;
    public event Action<PowerUpType> OnPowerUpExpired;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------------------------------------------------
    // ПУБЛИЧНЫЙ МЕТОД — АКТИВАЦИЯ УСИЛИТЕЛЯ
    // ---------------------------------------------------------
    public void ActivatePowerUp(PowerUpItem item)
    {
        if (item == null || item.type == PowerUpType.None)
            return;

        // если уже активен — перезапускаем таймер
        if (activeCoroutines.ContainsKey(item.type))
        {
            StopCoroutine(activeCoroutines[item.type]);
            activeCoroutines.Remove(item.type);
            activeItems.Remove(item.type);
        }

        activeItems[item.type] = item;
        activeCoroutines[item.type] = StartCoroutine(PowerUpRoutine(item));

        ApplyEffect(item);
        OnPowerUpActivated?.Invoke(item);
    }

    // ---------------------------------------------------------
    // ОСНОВНОЙ ТАЙМЕР
    // ---------------------------------------------------------
    private IEnumerator PowerUpRoutine(PowerUpItem item)
    {
        float t = item.duration;

        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            yield return null;
        }

        DisableEffect(item.type);

        activeCoroutines.Remove(item.type);
        activeItems.Remove(item.type);

        OnPowerUpExpired?.Invoke(item.type);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindPlayer();
    }

    private void TryFindPlayer()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            if (player != null)
                Debug.Log("PowerUpManager: PlayerController найден");
            else
                Debug.LogWarning("PowerUpManager: PlayerController НЕ найден");
        }
    }

    // ---------------------------------------------------------
    // ВКЛЮЧЕНИЕ ЭФФЕКТА
    // ---------------------------------------------------------
    private void ApplyEffect(PowerUpItem item)
    {
        switch (item.type)
        {
            case PowerUpType.Magnet:
                StartCoroutine(MagnetRoutine(item.magnetRadius));
                break;

         
            case PowerUpType.SpeedBoost:
                if (player != null)
                    player.forwardSpeed *= item.speedMultiplier;
                break;

            case PowerUpType.DoubleScore:
                ScoreManager.Instance?.SetMultiplier(item.scoreMultiplier);
                break;

            case PowerUpType.SlowMotion:
                SlowMoController.Instance?.TriggerPortalSlowMo();
                break;

            case PowerUpType.JumpBoost:
                PlayerJump.Instance?.EnableJumpBoost(item.duration);
                break;
        }
    }

    // ---------------------------------------------------------
    // ОТКЛЮЧЕНИЕ ЭФФЕКТА
    // ---------------------------------------------------------
    private void DisableEffect(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.SpeedBoost:
                if (player != null)
                    player.forwardSpeed /= activeItems[type].speedMultiplier;
                break;

            case PowerUpType.DoubleScore:
                ScoreManager.Instance?.ResetMultiplier();
                break;

            case PowerUpType.Magnet:
                // магнит отключается сам, когда корутина завершается
                break;

            case PowerUpType.SlowMotion:
                // SlowMoController сам завершает эффект
                break;

          

            case PowerUpType.JumpBoost:
                PlayerJump.Instance?.DisableJumpBoost();
                break;
        }
    }

    // ---------------------------------------------------------
    // МАГНИТ — ПРИТЯГИВАНИЕ ГЕМОв
    // ---------------------------------------------------------
    private IEnumerator MagnetRoutine(float radius)
    {
    //    Debug.Log("MAGNET STARTED");

        while (activeItems.ContainsKey(PowerUpType.Magnet))
        {
            if(player == null)
            {
                Debug.LogWarning("PowerUpManager: PlayerController не найден для магнита");
                yield break;
            }
            Collider[] hits = Physics.OverlapSphere(player.transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Gem"))
                {
                    Transform gem = hit.transform;

                    gem.position = Vector3.MoveTowards(
                        gem.position,
                        player.transform.position,
                        magnetForce * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

      //  Debug.Log("MAGNET STOPPED");
    }

}
