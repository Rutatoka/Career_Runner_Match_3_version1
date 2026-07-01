using UnityEngine;

[CreateAssetMenu(menuName = "Game/Profession Object", fileName = "NewProfessionObject")]
public class ProfessionObjectData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Название объекта (например: Ноутбук, Кисть, Мегафон)")]
    public string objectName;

    [Header("Profession Link")]
    [Tooltip("К какой профессии относится объект (IT, Design, Marketing...)")]
    public ProfessionType professionType;

    [Tooltip("Ссылка на ProfessionData (для портала, рекомендаций, карточек)")]
    public ProfessionData professionData;

    [Header("Visuals")]
    [Tooltip("Префаб объекта, который будет лежать на трассе")]
    public GameObject prefab;

    [Tooltip("Иконка для UI (слоты, подсказки, коллекция)")]
    public Sprite iconBg;
    public Sprite iconItem;

    [Tooltip("Цвет направления (из GDD: синий IT, фиолетовый Design и т.д.)")]
    public Color directionColor = Color.white;

    [Header("Thought Lines")]
    [Tooltip("Реплики персонажа при подборе этого объекта")]
    [TextArea(2, 5)]
    public string[] thoughtLines;

    [Header("Spawn Settings")]
    [Tooltip("Редкость появления объекта (0–1). 1 = очень часто, 0.1 = редко")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f;

    [Header("Debug")]
    public bool debugLogOnPickup = false;

    public string GetRandomThought()
    {
        if (thoughtLines == null || thoughtLines.Length == 0)
            return null;

        return thoughtLines[Random.Range(0, thoughtLines.Length)];
    }
}

public enum ProfessionType
{
    None = -1,
    IT = 0,
    Design = 1,
    Marketing = 2,
    Analytics = 3,
    Media = 4,
    Engineering = 5,
    Management = 6
}
