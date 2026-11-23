
using UnityEngine;
using UnityEngine.UI;
using Dino.Utility.Audio;

// Indicar cual AudioType vamos a utilizar
using AudioType = Dino.Utility.Audio.AudioType;

public class SettingsUI : UIWindow
{
    [Header("Sliders de volumen")]
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Slider _sfxSlider;
    private Slider vibrationSlider;
    private bool settingsInitialized;

    public override void Initialize()
    {
        if (settingsInitialized) return;
        settingsInitialized = true;
        base.Initialize();

        float musica = PlayerPrefs.GetFloat(
            "VolumenMusica", 1f);

        float efectos = PlayerPrefs.GetFloat(
            "VolumenEfectos", 1f);

        if (_volumeSlider != null)
        {
            _volumeSlider.minValue = 0f;
            _volumeSlider.maxValue = 1f;

            _volumeSlider.SetValueWithoutNotify(musica);

            _volumeSlider.onValueChanged.AddListener(
                CambiarMusica);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.minValue = 0f;
            _sfxSlider.maxValue = 1f;

            _sfxSlider.SetValueWithoutNotify(efectos);

            _sfxSlider.onValueChanged.AddListener(
                CambiarEfectos);
        }

        // Reuse the existing third control, including when its canvas is hidden.
        foreach (Slider slider in GetComponentsInChildren<Slider>(true))
        {
            if (slider == _volumeSlider || slider == _sfxSlider) continue;
            vibrationSlider = slider;
            vibrationSlider.minValue = 0f;
            vibrationSlider.maxValue = 1f;
            vibrationSlider.wholeNumbers = true;
            vibrationSlider.SetValueWithoutNotify(PlayerPrefs.GetInt("Vibracion", 1));
            vibrationSlider.onValueChanged.AddListener(CambiarVibracion);
            break;
        }

        CambiarMusica(musica);
        CambiarEfectos(efectos);
    }

    private void CambiarVibracion(float valor)
    {
        PlayerPrefs.SetInt("Vibracion", valor >= 0.5f ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CambiarMusica(float valor)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateAudioMixerGroupVolume(
                valor, AudioType.Music);
        }

        PlayerPrefs.SetFloat("VolumenMusica", valor);
        PlayerPrefs.Save();
    }

    private void CambiarEfectos(float valor)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateAudioMixerGroupVolume(
                valor, AudioType.SFX);
        }

        PlayerPrefs.SetFloat("VolumenEfectos", valor);
        PlayerPrefs.Save();
    }
}

