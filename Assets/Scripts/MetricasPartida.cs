using TMPro;
using UnityEngine;

/// <summary>One world unit is one virtual metre. Session and lifetime statistics.</summary>
public class MetricasPartida : MonoBehaviour
{
    public static MetricasPartida Instance { get; private set; }
    public const string ClaveMonedas = "TapWing.MonedasTotal";
    public const string ClaveDistancia = "TapWing.RecordDistancia";
    public GeneradorObstaculos generador;
    public TMP_Text textoMonedas, textoDistancia, textoTotalMonedas, textoResumenDerrota;
    public int Monedas { get; private set; }
    public int TotalMonedas { get; private set; }
    public float Distancia { get; private set; }
    public float RecordDistancia { get; private set; }
    private int metrosMostrados = -1;

    void Awake()
    {
        Instance = this;
        TotalMonedas = Mathf.Max(0, PlayerPrefs.GetInt(ClaveMonedas, 0));
        RecordDistancia = Mathf.Max(0, PlayerPrefs.GetFloat(ClaveDistancia, 0));
        ActualizarTextos();
    }

    void Update()
    {
        if (generador != null) Avanzar(generador.VelocidadActual, Time.deltaTime);
    }

    public void Avanzar(float velocidad, float segundos)
    {
        if (GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
        Distancia += Mathf.Max(0, velocidad) * Mathf.Max(0, segundos);
        RecordDistancia = Mathf.Max(RecordDistancia, Distancia);
        int metros = Mathf.FloorToInt(Distancia);
        if (metros != metrosMostrados)
        {
            metrosMostrados = metros;
            if (textoDistancia != null) textoDistancia.text = $"{metros} m";
        }
    }

    public void ReiniciarPartida()
    {
        Guardar();
        Monedas = 0;
        Distancia = 0;
        metrosMostrados = -1;
        ActualizarTextos();
    }

    public void RecogerMoneda()
    {
        if (GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
        Monedas++;
        TotalMonedas++;
        Guardar();
        ActualizarTextos();
    }

    public void Guardar()
    {
        PlayerPrefs.SetInt(ClaveMonedas, TotalMonedas);
        PlayerPrefs.SetFloat(ClaveDistancia, RecordDistancia);
        PlayerPrefs.Save();
    }

    public void ActualizarTextos()
    {
        if (textoMonedas != null) textoMonedas.text = Monedas.ToString();
        if (textoDistancia != null) textoDistancia.text = $"{Mathf.FloorToInt(Distancia)} m";
        if (textoTotalMonedas != null) textoTotalMonedas.text = $"Monedas acumuladas: {TotalMonedas}";
        if (textoResumenDerrota != null)
            textoResumenDerrota.text = $"Distancia: {Mathf.FloorToInt(Distancia)} m\nRécord de distancia: {Mathf.FloorToInt(RecordDistancia)} m\nMonedas de la partida: {Monedas}";
    }

    void OnApplicationPause(bool paused) { if (paused) Guardar(); }
    void OnApplicationQuit() { Guardar(); }
    void OnDestroy() { if (Instance == this) Instance = null; }
}
