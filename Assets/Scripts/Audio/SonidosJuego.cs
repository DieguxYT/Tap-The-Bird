
using UnityEngine;
using Dino.Utility.Audio;

public class SonidosJuego : MonoBehaviour
{
    public static SonidosJuego Instance;

    void Awake()
    {
        Instance = this;
    }

    public void IniciarMusica()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("MusicaFondo");
        }
    }

    public void DetenerMusica()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSound("MusicaFondo");
        }
    }

    public void SonidoVuelo()
    {
        Reproducir("Vuelo");
    }

    public void SonidoPunto()
    {
        Reproducir("Punto");
    }

    public void SonidoDerrota()
    {
        Reproducir("Derrota");
    }

    private void Reproducir(string nombre)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(nombre);
        }
    }
}
