
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Objetos del juego")]
    public GameObject player;
    public GameObject generadorObstaculos;

    [Header("Interfaz")]
    public UIManager UIManager;

    public string menuID = "MenuUI";
    public string pausaID = "PausaUI";
    public string derrotaID = "DerrotaUI";

    private Vector3 posicionInicial;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        posicionInicial = player.transform.position;
        MostrarMenu();
    }

    public void MostrarMenu()
    {
        Time.timeScale = 0f;

        player.SetActive(false);
        generadorObstaculos.SetActive(false);

        EliminarObstaculos();
        OcultarPantallas();

        UIManager.ShowWindow(menuID);

        if (SonidosJuego.Instance != null)
        {
            SonidosJuego.Instance.DetenerMusica();
        }
    }

    public void Jugar()
    {
        ReiniciarPartida();
    }

    public void Pausar()
    {
        Time.timeScale = 0f;
        UIManager.ShowWindow(pausaID);
    }

    public void Continuar()
    {
        UIManager.HideWindow(pausaID);
        Time.timeScale = 1f;
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;

        if (SonidosJuego.Instance != null)
        {
            SonidosJuego.Instance.IniciarMusica();
        }

        // Reiniciar puntuacion
        if (Puntuacion.Instance != null)
        {
            Puntuacion.Instance.ReiniciarPuntos();
        }

        EliminarObstaculos();

        // Reiniciar pajaro
        player.SetActive(false);
        player.transform.position = posicionInicial;
        player.SetActive(true);

        // Reiniciar generador
        generadorObstaculos.SetActive(false);
        generadorObstaculos.SetActive(true);

        OcultarPantallas();
    }

    public void Perder()
    {
        Time.timeScale = 0f;

        if (Puntuacion.Instance != null)
        {
            Puntuacion.Instance.ActualizarTextos();
        }

        UIManager.ShowWindow(derrotaID);
    }

    void EliminarObstaculos()
    {
        Obstaculo[] obstaculos =
            FindObjectsByType<Obstaculo>(
                FindObjectsSortMode.None);

        foreach (Obstaculo obstaculo in obstaculos)
        {
            Destroy(obstaculo.gameObject);
        }
    }

    void OcultarPantallas()
    {
        if (UIManager == null || UIManager.UIWindows == null)
        {
            Debug.LogError("Revisa el UIManager");
            return;
        }

        foreach (UIWindow ventana in UIManager.UIWindows)
        {
            if (ventana != null)
            {
                ventana.Hide(true);
            }
        }
    }


    public void AbrirConfiguracion()
    {
        UIManager.HideWindow(menuID);
        UIManager.ShowWindow("SettingsUI");
    }

    public void CerrarConfiguracion()
    {
        UIManager.HideWindow("SettingsUI");
        UIManager.ShowWindow(menuID);
    }

}


