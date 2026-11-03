
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

    public override void Initialize()
    {
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

        CambiarMusica(musica);
        CambiarEfectos(efectos);
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

