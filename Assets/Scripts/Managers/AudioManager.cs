using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Database")]
    public AudioDatabase database;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    public float fadeDuration = 1.0f;

    private AudioSource musicSource;
    private AudioSource musicSourceB;

    // Активный источник (A/B для кроссфейда)
    private bool isSourceA = true;

    private AudioSource ActiveSource => isSourceA ? musicSource : musicSourceB;
    private AudioSource InactiveSource => isSourceA ? musicSourceB : musicSource;

    // Трек который сейчас играет
    private AudioClip currentClip;

    // Индекс текущего game трека (0 или 1) — чередуем
    private int gameTrackIndex = 0;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Создаём два AudioSource для кроссфейда
        musicSource = CreateSource("MusicSource_A");
        musicSourceB = CreateSource("MusicSource_B");
    }

    private AudioSource CreateSource(string sourceName)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = true;
        src.volume = 0f;
        src.playOnAwake = false;
        return src;
    }

    // ─── ПУБЛИЧНЫЙ API ────────────────────────────────────────────────

    public void PlayMainMenu()
    {
        PlayClip(database?.mainMenuMusic);
    }

    public void PlayGame()
    {
        // Сбрасываем индекс на первый трек
        gameTrackIndex = 0;
        PlayClip(GetCurrentGameTrack());
    }

    public void PlayNextGameTrack()
    {
        // Вызывается когда трек заканчивается — переключаем на второй и обратно
        gameTrackIndex = (gameTrackIndex + 1) % 2;
        PlayClip(GetCurrentGameTrack());
    }

    public void PlayShop()
    {
        PlayClip(database?.shopMusic);
    }

    public void PlayChallengeAnalitics()
    {
        PlayClip(database?.Challenge_AnalyticsMusic);
    }
    public void PlayChallengeDesign()
    {
        PlayClip(database?.Challenge_DesignMusic);
    }
    public void PlayChallengeEngineer()
    {
        PlayClip(database?.Challenge_EngineerMusic);
    }
    public void PlayChallengeIT()
    {
        PlayClip(database?.Challenge_ITMusic);
    }
    public void PlayChallengeManagement()
    {
        PlayClip(database?.Challenge_ManagementMusic);
    }
    public void PlayChallengeMarketing()
    {
        PlayClip(database?.Challenge_MarketingMusic);
    }
    public void PlayChallengeMedia()
    {
        PlayClip(database?.Challenge_MediaMusic);
    }

    public void PlayMyPath()
    {
        PlayClip(database?.myPathMusic);
    }

    public void PlayDailyTasks()
    {
  //      PlayClip(database?.dailyTasksMusic);
    }

    public void StopMusic()
    {
        FadeOut(ActiveSource);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        musicSource.volume = musicVolume;
        musicSourceB.volume = musicVolume;
    }

    // ─── ВНУТРЕННЯЯ ЛОГИКА ────────────────────────────────────────────

    private AudioClip GetCurrentGameTrack()
    {
        if (database == null) return null;

        return gameTrackIndex == 0
            ? database.gameMusic1
            : database.gameMusic2;
    }

    private void PlayClip(AudioClip clip)
    {
        // Если тот же клип уже играет — не прерываем
        if (clip == null || clip == currentClip)
            return;

        currentClip = clip;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossFade(clip));
    }

    private IEnumerator CrossFade(AudioClip newClip)
    {
        var fadeOut = ActiveSource;
        isSourceA = !isSourceA;
        var fadeIn = ActiveSource;

        // Запускаем новый трек на тихом источнике
        fadeIn.clip = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            fadeIn.volume = Mathf.Lerp(0f, musicVolume, t);
            fadeOut.volume = Mathf.Lerp(musicVolume, 0f, t);

            yield return null;
        }

        fadeIn.volume = musicVolume;
        fadeOut.volume = 0f;
        fadeOut.Stop();

        fadeCoroutine = null;
    }

    private void FadeOut(AudioSource source)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutCoroutine(source));
    }

    private IEnumerator FadeOutCoroutine(AudioSource source)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f,
                Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        currentClip = null;
        fadeCoroutine = null;
    }

    // ─── АВТОПЕРЕКЛЮЧЕНИЕ ТРЕКОВ В ИГРЕ ──────────────────────────────
    // Проверяем в Update когда game трек закончился — переключаем на следующий
    private void Update()
    {
        // Переключаем игровые треки только если активный источник
        // перестал играть и мы в игровом состоянии
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameManager.GameState.Game) return;

        if (!ActiveSource.isPlaying && currentClip != null)
            PlayNextGameTrack();
    }
}