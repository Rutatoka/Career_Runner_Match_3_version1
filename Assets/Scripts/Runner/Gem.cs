using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Gem : MonoBehaviour
{
    [Header("Collection")]
    public bool collectOnTrigger = true;
    public float missedOffsetZ = 10f;

    [Header("Values")]
    public int happinessValue = 1;
    public int gemValue = 0;

    private bool collected = false;
    private Transform player;

    private void Start()
    {
     
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;

        if (collectOnTrigger)
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger) col.isTrigger = true;
        }

        // Ensure tag
        if (gameObject.tag != "Gem")
        {
            try { gameObject.tag = "Gem"; } catch { }
        }   
        OnSpawned();
    }

    private void Update()
    {
        if (collected) return;
        if (player == null) return;
        if (Mathf.Abs(player.position.x - transform.position.x) < 0.5f &&
            player.position.z > transform.position.z + missedOffsetZ)
        {
            Missed();
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (!collectOnTrigger || collected) return;
        if (other.CompareTag("Player"))
            Collect();
    }

    public void OnSpawned()
    {
        collected = false;
        collectOnTrigger = true;

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }


    public void Collect()
    {
        if (collected) return;
        collected = true;

        if (HappinessSystem.Instance != null)
        {
            HappinessSystem.Instance.Add(happinessValue);
        }
        else
        {
            SaveSystem.AddGems(gemValue);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateDailyTaskProgress("earn_coins", 1);
        }

        TryDespawnOrDestroy();
    }

    private void Missed()
    {
        if (collected) return;
        collected = true;

        if (HappinessSystem.Instance != null)
        {
            HappinessSystem.Instance.Add(-happinessValue);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateDailyTaskProgress("earn_coins", -1);
        }

        TryDespawnOrDestroy();
    }

    private void TryDespawnOrDestroy()
    {
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && pooled.originalPrefab != null && SimplePool.Instance != null)
        {
            SimplePool.Instance.Despawn(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
