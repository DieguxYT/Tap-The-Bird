
using UnityEngine;
using TMPro;

public class Puntuacion : MonoBehaviour
{
    public static Puntuacion Instance;

    [Header("Textos de la interfaz")]
    public TMP_Text textoPuntos;
    public TMP_Text textoPuntosDerrota;
    public TMP_Text textoRecord;
    public TMP_Text textoRecordMenu;

    private int puntos = 0;
    private int record = 0;

    void Awake()
    {
        Instance = this;

        // Recuperar el record guardado
        record = PlayerPrefs.GetInt("RecordTapWing", 0);
    }

    void Start()
    {
        ActualizarTextos();
    }

    public void SumarPunto()
    {
        puntos++;

        if (puntos > record)
        {
            record = puntos;

            PlayerPrefs.SetInt("RecordTapWing", record);
            PlayerPrefs.Save();
        }

        ActualizarTextos();

        Debug.Log("Puntos: " + puntos);


        GeneradorObstaculos generador =
            FindFirstObjectByType<GeneradorObstaculos>();

        if (generador != null)
        {
            generador.ActualizarPuntos(puntos);
        }

    }

    public void ReiniciarPuntos()
    {
        puntos = 0;
        ActualizarTextos();
    }

    public void ActualizarTextos()
    {
        if (textoPuntos != null)
            textoPuntos.text = puntos.ToString();

        if (textoPuntosDerrota != null)
            textoPuntosDerrota.text = "Puntos: " + puntos;

        if (textoRecord != null)
            textoRecord.text = "Récord: " + record;
        if (textoRecordMenu != null) textoRecordMenu.text = record.ToString();
    }
}

