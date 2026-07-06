using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase",
    menuName = "Game/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Header("BGM Ч Main Menu")]
    public AudioClip mainMenuMusic;

    [Header("BGM Ч Game (два трека чередуютс€)")]
    public AudioClip gameMusic1;
    public AudioClip gameMusic2;

    [Header("BGM Ч Shop / Profile")]
    public AudioClip shopMusic;

    [Header("BGM Ч Challenge (мини-игры)")]
    public AudioClip Challenge_AnalyticsMusic;
    public AudioClip Challenge_DesignMusic;
    public AudioClip Challenge_EngineerMusic;
    public AudioClip Challenge_ITMusic;
    public AudioClip Challenge_ManagementMusic;
    public AudioClip Challenge_MarketingMusic;
    public AudioClip Challenge_MediaMusic;

    [Header("BGM Ч MyPath / LinkToCourse")]
    public AudioClip myPathMusic;

   // [Header("BGM Ч Daily Tasks / Settings")]
 //   public AudioClip dailyTasksMusic;
}