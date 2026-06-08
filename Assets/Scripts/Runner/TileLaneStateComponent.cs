using System.Collections.Generic;
using UnityEngine;

public class TileLaneStateComponent : MonoBehaviour
{
    public TileLaneState state;
    private readonly List<float> occupiedZ = new List<float>();

    [Header("Distances")]
    public float minZDistance = 2f; // Уменьшено с 8f до 2f!

    public void Init(int laneCount)
    {
        if (state == null || state.laneOccupied.Length != laneCount)
        {
            state = new TileLaneState(laneCount);
        }
        else
        {
            state.Clear();
        }
        occupiedZ.Clear();
    }

    public bool IsZFree(float worldZ)
    {
        foreach (float z in occupiedZ)
        {
            if (Mathf.Abs(z - worldZ) < minZDistance)
                return false;
        }
        return true;
    }

    public void OccupyZ(float worldZ)
    {
        occupiedZ.Add(worldZ);
    }
}

[System.Serializable]

public class TileLaneState
{
    public bool[] laneOccupied;
    private List<float>[] laneOccupiedZ; // Z-позиции для каждой линии

    public TileLaneState(int laneCount)
    {
        laneOccupied = new bool[laneCount];
        laneOccupiedZ = new List<float>[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            laneOccupiedZ[i] = new List<float>();
        }
    }

    public bool IsFree(int lane) => !laneOccupied[lane];

    public bool IsFreeAtZ(int lane, float z, float minDistance = 1.5f)
    {
        if (laneOccupied[lane]) return false;

        foreach (float occupiedZ in laneOccupiedZ[lane])
        {
            if (Mathf.Abs(occupiedZ - z) < minDistance)
                return false;
        }
        return true;
    }

    public void Occupy(int lane) => laneOccupied[lane] = true;

    public void OccupyAtZ(int lane, float z)
    {
        laneOccupiedZ[lane].Add(z);
    }

    public void Clear()
    {
        for (int i = 0; i < laneOccupied.Length; i++)
        {
            laneOccupied[i] = false;
            laneOccupiedZ[i].Clear();
        }
    }
}