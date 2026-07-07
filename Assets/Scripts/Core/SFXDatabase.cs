using UnityEngine;

[CreateAssetMenu(fileName = "SFXDatabase", menuName = "Game/SFX Database")]
public class SFXDatabase : ScriptableObject
{
    [Header("UI")]
    public AudioClip button;
    public AudioClip button2;
    public AudioClip swipe;

    [Header("Runner")]
    public AudioClip coin;
    public AudioClip portal;
    public AudioClip powerUp;
    public AudioClip profession;

    [Header("MiniGame")]
    public AudioClip rightAnswer;
    public AudioClip wrongAnswer;
    public AudioClip perfectNote;
    public AudioClip goodNote;
    public AudioClip badNote;
    public AudioClip bueItem;

    [Header("Results")]
    public AudioClip winResult;
    public AudioClip lossResult;

    [Header("Equipment")]
    public AudioClip equipItemToProfile;
    public AudioClip unequipItemToProfile;
}