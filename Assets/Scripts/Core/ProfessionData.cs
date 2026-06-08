using UnityEngine;

[CreateAssetMenu(fileName = "ProfessionData", menuName = "Game/Profession Data")]
public class ProfessionData : ScriptableObject
{
    public enum ProfessionCategory
    {
        None,
        Key,
        Creative,
        Social,
        Business,
        Specialization,
        Additional
    }
    public string courseURL;
    public bool isCategory;
    [Header("Skill Vector (для матчинга)")]
    public float[] skillVector;
    public ProfessionType type;
    public Sprite icon;

    [Header("Base")]
    public string professionName;
    public ProfessionCategory category;

    // остальное, что у тебя уже есть:
    // public ProfessionType professionType;
    // public Color directionColor;
    // public string objectName;
    public float[] GetVectorCopy()
    {
        if (skillVector == null)
            return new float[0];

        float[] copy = new float[skillVector.Length];
        for (int i = 0; i < skillVector.Length; i++)
            copy[i] = skillVector[i];

        return copy;
    }

    // и т.д.
}
