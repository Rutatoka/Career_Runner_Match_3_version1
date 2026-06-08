using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChallengeEngineerController : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 4;
    public int gridHeight = 4;
    public Vector2 tileSpacing = new Vector2(150f, 150f);
    public GameObject gearTilePrefab;

    [Header("Challenge Settings")]
    public float timeLimit = 60f;
    public Vector2Int sourcePos = new Vector2Int(3, 0);
    public Vector2Int drainPos = new Vector2Int(0, 3);
    public Color lockedTileColor = new Color(0.3f, 0.5f, 1f, 0.8f);
    private bool ended;
    [Header("References")]
    public ResultWindow resultWindow;
    public TMP_Text timerText;
    public RectTransform gridParent;

    private GearTile[,] grid;
    private float timeLeft;
    private bool finished = false;
    public List<Vector2Int> solutionPath = new List<Vector2Int>();
    private void Start()
    {
        timeLeft = timeLimit;
        if (resultWindow == null) resultWindow = FindObjectOfType<ResultWindow>();
        GenerateGrid();
        UpdateUI();
    }
    private List<Vector2Int> GenerateRandomPath()
    {
        List<Vector2Int> path = new();
        HashSet<Vector2Int> visited = new();

        bool success = FindPath(sourcePos, path, visited);

        return success ? path : null;
    }
    private bool FindPath(
    Vector2Int current,
    List<Vector2Int> path,
    HashSet<Vector2Int> visited)
    {
        path.Add(current);
        visited.Add(current);

        if (current == drainPos)
            return true;

        List<Vector2Int> neighbors = new()
    {
        current + Vector2Int.up,
        current + Vector2Int.right,
        current + Vector2Int.down,
        current + Vector2Int.left
    };

        for (int i = 0; i < neighbors.Count; i++)
        {
            int rnd = Random.Range(i, neighbors.Count);
            (neighbors[i], neighbors[rnd]) =
                (neighbors[rnd], neighbors[i]);
        }

        foreach (var next in neighbors)
        {
            if (!InBounds(next))
                continue;

            if (visited.Contains(next))
                continue;

            if (FindPath(next, path, visited))
                return true;
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }
    private void Update()
    {
        if (finished) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            EndGame(false, "time");
            return;
        }
        UpdateUI();
    }

    // ==================== ГЕНЕРАЦИЯ (ПРОСТАЯ И РАБОЧАЯ) ====================
    private void GenerateGrid()
    {
        grid = new GearTile[gridWidth, gridHeight];
        sourcePos = new Vector2Int(
    Random.Range(0, gridWidth),
    0
);

        drainPos = new Vector2Int(
            Random.Range(0, gridWidth),
            gridHeight - 1
        );
        // 1. Создаём тайлы
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 pos = new Vector2(x * tileSpacing.x, y * tileSpacing.y);
                GameObject obj = Instantiate(gearTilePrefab, gridParent);
                obj.GetComponent<RectTransform>().anchoredPosition = pos;
                GearTile tile = obj.GetComponent<GearTile>();
                tile.Init(this, new Vector2Int(x, y));
                grid[x, y] = tile;
            }
        }

        // 2. Путь (любой, например L-образный)
        solutionPath = GenerateRandomPath();

        // 3. Настраиваем правильные соединения на пути
        foreach (Vector2Int pos in solutionPath)
        {
            GearTile t = grid[pos.x, pos.y];
            t.ClearConnections();

            int index = solutionPath.IndexOf(pos);

            // Связь с предыдущей
            if (index > 0)
            {
                Vector2Int prev = solutionPath[index - 1];
                Vector2Int dir = prev - pos;
                if (dir == new Vector2Int(0, 1)) t.SetConnection(0, true);
                else if (dir == new Vector2Int(1, 0)) t.SetConnection(1, true);
                else if (dir == new Vector2Int(0, -1)) t.SetConnection(2, true);
                else if (dir == new Vector2Int(-1, 0)) t.SetConnection(3, true);
            }

            // Связь со следующей
            if (index < solutionPath.Count - 1)
            {
                Vector2Int next = solutionPath[index + 1];
                Vector2Int dir = next - pos;
                if (dir == new Vector2Int(0, 1)) t.SetConnection(0, true);
                else if (dir == new Vector2Int(1, 0)) t.SetConnection(1, true);
                else if (dir == new Vector2Int(0, -1)) t.SetConnection(2, true);
                else if (dir == new Vector2Int(-1, 0)) t.SetConnection(3, true);
            }

            t.UpdateConnectorVisuals();

            // Блокируем Source и Drain
            if (pos == sourcePos || pos == drainPos)
            {
                t.SetLocked(true);
                t.SetBackgroundColor(lockedTileColor);
            }
        }

        // 4. ? СЛУЧАЙНО ПОВОРАЧИВАЕМ ТРУБЫ НА ПУТИ (КРОМЕ ВХОДА И ВЫХОДА)
        foreach (Vector2Int pos in solutionPath)
        {
            if (pos == sourcePos || pos == drainPos) continue;

            GearTile t = grid[pos.x, pos.y];
            int rotations = Random.Range(1, 4); // 1, 2 или 3 поворота (не 0!)
            for (int i = 0; i < rotations; i++)
            {
                // Ручной поворот массива connections
                bool temp = t.connections[3];
                for (int d = 3; d > 0; d--) t.connections[d] = t.connections[d - 1];
                t.connections[0] = temp;
            }
            t.UpdateConnectorVisuals();
        }

        // 5. Заполняем остальные клетки случайными соединениями
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int p = new Vector2Int(x, y);

                if (solutionPath.Contains(p))
                    continue;

                GearTile tile = grid[x, y];

                tile.ClearConnections();

                List<int> dirs = new() { 0, 1, 2, 3 };

                int count = Random.Range(1, 4);

                for (int i = 0; i < count; i++)
                {
                    int index = Random.Range(0, dirs.Count);

                    tile.SetConnection(dirs[index], true);

                    dirs.RemoveAt(index);
                }

                tile.UpdateConnectorVisuals();
            }
        }

        UpdateAllVisuals();
    }
    // ==================== ИГРОВАЯ ЛОГИКА ====================
    public void OnTileRotated(Vector2Int coords)
    {
        if (finished) return;

        UpdateAllVisuals();

        if (CheckPath())
            EndGame(true, "success");
    }

    private bool CheckPath()
    {
        // Проходим по всему пути и проверяем каждое соединение
        for (int i = 0; i < solutionPath.Count - 1; i++)
        {
            Vector2Int current = solutionPath[i];
            Vector2Int next = solutionPath[i + 1];
            Vector2Int dir = next - current;

            // Направление от current к next
            int neededDir = -1;
            if (dir == new Vector2Int(0, 1)) neededDir = 0; // up
            else if (dir == new Vector2Int(1, 0)) neededDir = 1; // right
            else if (dir == new Vector2Int(0, -1)) neededDir = 2; // down
            else if (dir == new Vector2Int(-1, 0)) neededDir = 3; // left

            GearTile currentTile = grid[current.x, current.y];
            GearTile nextTile = grid[next.x, next.y];

            // Проверяем, что currentTile смотрит в сторону next
            if (!currentTile.HasConnection(neededDir)) return false;

            // Проверяем, что nextTile смотрит в сторону current
            int oppositeDir = (neededDir + 2) % 4;
            if (!nextTile.HasConnection(oppositeDir)) return false;
            if (!nextTile.HasConnection(oppositeDir))
            {
                Debug.Log($"Ошибка между {current} и {next}");
                return false;
            }

        }
        return true;
    }
    // ==================== ВСПОМОГАТЕЛЬНЫЕ ====================
    public GearTile GetTile(Vector2Int pos)
    {
        if (InBounds(pos)) return grid[pos.x, pos.y];
        return null;
    }

    private bool InBounds(Vector2Int p) =>
        p.x >= 0 && p.x < gridWidth && p.y >= 0 && p.y < gridHeight;
    private void UpdateAllVisuals()
    {
        foreach (var t in grid)
        {
            t.UpdateConnectorVisuals();
         //   t.UpdateValidationHighlight();
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }
    private IEnumerator ShowResultDelayed(bool success, string reason)
    {
        yield return new WaitForSeconds(0.5f);

        if (resultWindow == null)
            yield break;

        if (success)
        {
            resultWindow.ShowSuccess(
                gridWidth * gridHeight,
                gridWidth * gridHeight,
                timeLeft
            );
        }
        else
        {
            resultWindow.ShowFailure(
                0,
                gridWidth * gridHeight,
                reason
            );
        }
    }
    // ==================== ФИНАЛ (ТВОЙ, БЕЗ ИЗМЕНЕНИЙ) ====================
    private void EndGame(bool success, string reason)
    {
        if (ended) return;
        ended = true;

        finished = true;

        Debug.Log($"[ChallengeEngineer] EndGame success={success}, reason={reason}");

        foreach (var t in grid)
            t.SetLocked(true);

        ChallengeManager.Instance?.FinishChallenge(success);

        StartCoroutine(ShowResultDelayed(success, reason));
    }
}