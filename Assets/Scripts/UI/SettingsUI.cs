using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider slowMoSlider;
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    private float lastMusicVolume = 0.7f;
    private float lastSFXVolume = 1f;
    private void Start()
    {
        float musicVolume = SaveSystem.GetMusicVolume();
        float sfxVolume = SaveSystem.GetSFXVolume();
        float slowScale = SaveSystem.GetSlowMoScale(); 
        lastMusicVolume = musicVolume;
        lastSFXVolume = sfxVolume;

        musicMuteToggle.SetIsOnWithoutNotify(musicVolume <= 0.001f);
        sfxMuteToggle.SetIsOnWithoutNotify(sfxVolume <= 0.001f);

        musicMuteToggle.onValueChanged.AddListener(OnMusicMuteChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);
        slowMoSlider.SetValueWithoutNotify(slowScale);

        SlowMoController.Instance?.SetSlowScale(slowScale);

        slowMoSlider.onValueChanged.AddListener(OnSlowMoChanged);
        musicSlider.SetValueWithoutNotify(musicVolume);
        sfxSlider.SetValueWithoutNotify(sfxVolume);

        AudioManager.Instance?.SetMusicVolume(musicVolume);
        SFXManager.Instance?.SetVolume(sfxVolume);

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    private void OnDestroy()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged); 
        slowMoSlider.onValueChanged.RemoveListener(OnSlowMoChanged);
        musicMuteToggle.onValueChanged.RemoveListener(OnMusicMuteChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);
    }
    private void OnSlowMoChanged(float value)
    {
        SlowMoController.Instance?.SetSlowScale(value);
        SaveSystem.SetSlowMoScale(value);
    }
    private void OnMusicChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        SaveSystem.SetMusicVolume(value);
        musicMuteToggle.SetIsOnWithoutNotify(value <= 0.001f);
    }
    private void OnSFXChanged(float value)
    {
        SFXManager.Instance?.SetVolume(value);

        SaveSystem.SetSFXVolume(value);

        sfxMuteToggle.SetIsOnWithoutNotify(value <= 0.001f);
    }
    private void OnMusicMuteChanged(bool mute)
    {
        if (mute)
        {
            lastMusicVolume = musicSlider.value;
            musicSlider.value = 0f;
        }
        else
        {
            musicSlider.value = Mathf.Max(lastMusicVolume, 0.1f);
        }
    }

    private void OnSFXMuteChanged(bool mute)
    {
        if (mute)
        {
            lastSFXVolume = sfxSlider.value;
            sfxSlider.value = 0f;
        }
        else
        {
            sfxSlider.value = Mathf.Max(lastSFXVolume, 0.1f);
        }
    }

}