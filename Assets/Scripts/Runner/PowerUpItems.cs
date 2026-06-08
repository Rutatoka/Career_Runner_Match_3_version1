using UnityEngine;

[CreateAssetMenu(menuName = "Game/PowerUp Item", fileName = "NewPowerUp")]
public class PowerUpItem : ScriptableObject
{
    [Header("Identity")]
    public PowerUpType type;
    public string displayName;

    [Header("Visuals")]
    public Sprite icon;
    public Color uiColor = Color.white;
    public GameObject worldPrefab;

    [Header("Duration")]
    [Tooltip("Сколько секунд длится эффект усилителя")]
    public float duration = 5f;

    [Header("Effect Parameters")]
    [Tooltip("Для магнита: радиус притяжения")]
    public float magnetRadius = 6f;

    [Tooltip("Для ускорения: множитель скорости")]
    public float speedMultiplier = 1.5f;

    [Tooltip("Для щита: игнорирование препятствий")]
    public bool grantsInvulnerability = false;

    [Tooltip("Для двойных очков: множитель")]
    public float scoreMultiplier = 2f;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float spawnWeight = 0.3f;

    public override string ToString()
    {
        return $"{type} ({duration}s)";
    }
}

public enum PowerUpType
{
    None = -1,
    Magnet,
    Shield,
    SpeedBoost,
    DoubleScore,
    SlowMotion,
    JumpBoost
}
