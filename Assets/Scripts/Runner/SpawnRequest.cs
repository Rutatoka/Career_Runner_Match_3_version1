using UnityEngine;
using System;

public class SpawnRequest
{
    public GameObject prefab;
    public float height;
    public float zOffset;
    public Action<GameObject> onSpawn;
}
