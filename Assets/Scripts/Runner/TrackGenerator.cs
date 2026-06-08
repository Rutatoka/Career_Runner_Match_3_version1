
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [Header("Unified Spawner")]
    public TileObjectSpawner tileObjectSpawner;

    private void Awake()
    {
        // Автоматический поиск если не назначен
        if (tileObjectSpawner == null)
        {
            tileObjectSpawner = FindObjectOfType<TileObjectSpawner>();

            if (tileObjectSpawner == null)
            {
               // Debug.LogError("[TrackGenerator] TileObjectSpawner NOT FOUND in scene! Trying to create...");

                // Пробуем найти в родительских объектах
                tileObjectSpawner = GetComponentInParent<TileObjectSpawner>();

                if (tileObjectSpawner == null)
                {
                    // Создаём новый
                    GameObject go = new GameObject("TileObjectSpawner");
                    tileObjectSpawner = go.AddComponent<TileObjectSpawner>();
                  //  Debug.Log("[TrackGenerator] Created new TileObjectSpawner");
                }
            }
        }

     //   Debug.Log($"[TrackGenerator] TileObjectSpawner: {(tileObjectSpawner != null ? "ASSIGNED" : "NULL")}");
    }

    public void PopulateTile(GameObject tile, float tileWorldZ)
    {
        if (tile == null)
        {
           // Debug.LogError("[TrackGenerator] Tile is NULL!");
            return;
        }

        if (tileObjectSpawner == null)
        {
         //   Debug.LogError("[TrackGenerator] CRITICAL: TileObjectSpawner is NULL! Attempting to find...");
            tileObjectSpawner = FindObjectOfType<TileObjectSpawner>();

            if (tileObjectSpawner == null)
            {
                //Debug.LogError("[TrackGenerator] Cannot find TileObjectSpawner anywhere!");
                return;
            }
        }

      //  Debug.Log($"[TrackGenerator] Populating tile at z={tileWorldZ}, calling SpawnAll...");
        tileObjectSpawner.SpawnAll(tile.transform, tileWorldZ);
    }
}