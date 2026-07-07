using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Database")]
    public SFXDatabase database;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [SerializeField]
    private int audioSourceCount = 8;

    private AudioSource[] sources;
    private int currentSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateSources();
    }

    private void CreateSources()
    {
        sources = new AudioSource[audioSourceCount];

        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject go = new GameObject($"SFXSource_{i}");
            go.transform.SetParent(transform);

            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.volume = sfxVolume;

            sources[i] = source;
        }
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource source = sources[currentSource];

        source.volume = sfxVolume;
        source.PlayOneShot(clip);

        currentSource++;
        if (currentSource >= sources.Length)
            currentSource = 0;
    }

    public void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // UI
    public void PlayButton() => Play(database.button);
    public void PlayButton2() => Play(database.button2);
    public void PlaySwipe() => Play(database.swipe);

    // Runner
    public void PlayCoin() => Play(database.coin);
    public void PlayPortal() => Play(database.portal);
    public void PlayPowerUp() => Play(database.powerUp);
    public void PlayProfession() => Play(database.profession);

    // MiniGame
    public void PlayRightAnswer() => Play(database.rightAnswer);
    public void PlayWrongAnswer() => Play(database.wrongAnswer);
    public void PlayPerfectNote() => Play(database.perfectNote);
    public void PlayGoodNote() => Play(database.goodNote);
    public void PlayBadNote() => Play(database.badNote);

    // Results
    public void PlayWinResult() => Play(database.winResult);
    public void PlayLossResult() => Play(database.lossResult);

    // Equipment
    public void PlayEquipItem() => Play(database.equipItemToProfile);
    public void PlayUnequipItem() => Play(database.unequipItemToProfile);
    public void PlayBueItem() => Play(database.bueItem);
   
}