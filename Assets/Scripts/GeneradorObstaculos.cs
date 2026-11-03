

using UnityEngine;

public class GeneradorObstaculos : MonoBehaviour
{
    [Header("Obstaculos")]
    public GameObject obstaculoPrefab;

    public float tiempoGeneracion = 2f;
    public float posicionX = 10f;
    public float minimoY = -3f;
    public float maximoY = 3f;

    [Header("Dificultad")]
    public float velocidadInicial = 3f;
    public float aumentoVelocidad = 0.5f;
    public int puntosParaAumentar = 5;
    public float velocidadMaxima = 7f;

    private float tiempo = 0f;
    private int puntosActuales = 0;

    void OnEnable()
    {
        tiempo = 0f;
        puntosActuales = 0;
    }

    void Update()
    {
        tiempo += Time.deltaTime;

        if (tiempo >= tiempoGeneracion)
        {
            GenerarObstaculo();
            tiempo = 0f;
        }
    }

    void GenerarObstaculo()
    {
        float posicionY = Random.Range(minimoY, maximoY);

        Vector3 posicion = new Vector3(
            posicionX,
            posicionY,
            0f
        );

        GameObject nuevoObstaculo =
            Instantiate(obstaculoPrefab, posicion, Quaternion.identity);

        Obstaculo scriptObstaculo =
            nuevoObstaculo.GetComponent<Obstaculo>();

        if (scriptObstaculo != null)
        {
            float velocidadNueva =
                velocidadInicial +
                (puntosActuales / Mathf.Max(1, puntosParaAumentar))
                * aumentoVelocidad;

            velocidadNueva = Mathf.Min(
                velocidadNueva,
                velocidadMaxima
            );

            scriptObstaculo.velocidad = velocidadNueva;

            Debug.Log("Velocidad actual: " + velocidadNueva);
        }
    }

    public void ActualizarPuntos(int puntos)
    {
        puntosActuales = puntos;
    }
}
