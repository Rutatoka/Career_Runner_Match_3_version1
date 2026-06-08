using UnityEngine;
using System;
using TMPro;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance;

    [Header("References")]
    public PlayerController player;          // çàäà¸øü â èíñïåêòîğå
    public TextMeshProUGUI coinText;

    [Header("Settings")]
    public int startCoins = 0;

    [Header("Effects")]
    public AudioClip collectSound;
    public ParticleSystem collectEffect;

    private int coins;
    private float multiplier = 1f;

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        Instance = this;
        coins = startCoins;
        UpdateUI();
    }

    private void OnEnable()
    {
        if (player != null)
            player.OnCollectedGem += HandleCollectiblePickup;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnCollectedGem -= HandleCollectiblePickup;
    }

    // ---------------------------------------------------------
    // ÏÎÄÁÎĞ ÏĞÅÄÌÅÒÀ
    // ---------------------------------------------------------
    private void HandleCollectiblePickup(GameObject gem)
    {
        AddCoins(1);

        if (collectEffect != null)
            Instantiate(collectEffect, gem.transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, gem.transform.position);
    }

    // ---------------------------------------------------------
    // ÄÎÁÀÂËÅÍÈÅ ÌÎÍÅÒ
    // ---------------------------------------------------------
    public void AddCoins(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * multiplier);
        coins += finalAmount;

        OnCoinsChanged?.Invoke(coins);
        UpdateUI();
    }

    // ---------------------------------------------------------
    // ÌÍÎÆÈÒÅËÜ
    // ---------------------------------------------------------
    public void SetMultiplier(float value)
    {
        multiplier = value;
    }

    public void ResetMultiplier()
    {
        multiplier = 1f;
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString();
    }

    // ---------------------------------------------------------
    // ÏÎËÓ×ÅÍÈÅ ÄÀÍÍÛÕ
    // ---------------------------------------------------------
    public int GetCoins() => coins;

    public void SetCoins(int value)
    {
        coins = Mathf.Max(0, value);
        UpdateUI();
    }
}
