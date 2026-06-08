using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GearTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Visual Elements")]
    public Image gearImage;
    public Image[] connectors; // 0:Up, 1:Right, 2:Down, 3:Left
    public Image backgroundHighlight;
    public Color activeConnectorColor = Color.green;
    public Color inactiveConnectorColor = Color.gray;
    public Color validTileColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color invalidTileColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
    public Color defaultTileColor = Color.white;

    public float rotationDuration = 0.2f;

    private ChallengeEngineerController controller;
    private Vector2Int coords;
    public bool[] connections = new bool[4];
    private bool locked = false;

    public void Init(ChallengeEngineerController ctrl, Vector2Int pos)
    {
        controller = ctrl;
        coords = pos;
        if (backgroundHighlight) backgroundHighlight.color = defaultTileColor;
    }

    public void ClearConnections() { for (int i = 0; i < 4; i++) connections[i] = false; }
    public void SetConnection(int dir, bool active) => connections[dir] = active;
    public bool HasConnection(int dir) => connections[dir];

    public void UpdateValidationHighlight()
    {
        if (locked || backgroundHighlight == null) return;

        bool hasAnyConnection = false;
        bool isValid = true;

        for (int i = 0; i < 4; i++) if (connections[i]) hasAnyConnection = true;
        if (!hasAnyConnection) isValid = false;

        if (isValid)
        {
            for (int d = 0; d < 4; d++)
            {
                if (!connections[d]) continue;
                Vector2Int neighborPos = coords + DirToVec(d);
                GearTile neighbor = controller.GetTile(neighborPos);

                if (neighbor == null || !neighbor.HasConnection((d + 2) % 4))
                {
                    isValid = false;
                    break;
                }
            }
        }

        backgroundHighlight.color = isValid ? validTileColor : invalidTileColor;
    }

    Vector2Int DirToVec(int d) => d switch
    {
        0 => new Vector2Int(0, 1),
        1 => new Vector2Int(1, 0),
        2 => new Vector2Int(0, -1),
        _ => new Vector2Int(-1, 0)
    };

    void RotateCW()
    {
        // Сохраняем последний элемент
        bool temp = connections[3];
        // Сдвигаем все элементы вправо
        for (int i = 3; i > 0; i--)
        {
            connections[i] = connections[i - 1];
        }
        connections[0] = temp;
    }
    public void UpdateConnectorVisuals()
    {
        for (int i = 0; i < 4; i++)
            if (connectors[i]) connectors[i].color = connections[i] ? activeConnectorColor : inactiveConnectorColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (locked || controller == null) return;

        RotateCW();
        UpdateConnectorVisuals();
        controller.OnTileRotated(coords);
    }

    public void SetLocked(bool state) => locked = state;
    public void SetBackgroundColor(Color c) { if (backgroundHighlight) backgroundHighlight.color = c; }
    public Vector2Int GetCoords() => coords;
}