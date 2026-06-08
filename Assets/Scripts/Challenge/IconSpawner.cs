using UnityEngine;
using System.Collections;

public class IconSpawner : MonoBehaviour
{
    public GameObject likePrefab;
    public GameObject repostPrefab;
    public GameObject dislikePrefab;
    public GameObject angryPrefab;

    public RectTransform spawnArea;
    public float spawnInterval = 0.8f;

    private bool spawning = false;

    public void StartSpawning()
    {
        spawning = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            SpawnIcon();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnIcon()
    {
        int r = Random.Range(0, 4);
        GameObject prefab = r switch
        {
            0 => likePrefab,
            1 => repostPrefab,
            2 => dislikePrefab,
            _ => angryPrefab
        };

        RectTransform icon = Instantiate(prefab, spawnArea).GetComponent<RectTransform>();

        float x = Random.Range(-400f, 400f);
        icon.anchoredPosition = new Vector2(x, 900f);
    }
}
