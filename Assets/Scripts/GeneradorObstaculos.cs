

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

    [Header("Monedas")]
    public Moneda monedaPrefab;
    [Range(0f, 1f)] public float probabilidadMoneda = 0.7f;
    public float VelocidadActual => Mathf.Min(velocidadInicial +
        (puntosActuales / Mathf.Max(1, puntosParaAumentar)) * aumentoVelocidad, velocidadMaxima);

    private float tiempo = 0f;
    private int puntosActuales = 0;

    void OnEnable()
    {
        tiempo = 0f;
        puntosActuales = 0;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
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
        Camera vista = Camera.main;
        float origenX = posicionX;
        if (vista != null)
            origenX = Mathf.Max(posicionX, vista.transform.position.x + vista.orthographicSize * vista.aspect + 1.5f);

        Vector3 posicion = new Vector3(
            origenX,
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

        if (monedaPrefab != null && Random.value < probabilidadMoneda)
        {
            // Half a spawn interval behind this obstacle, in the central flight corridor.
            float altura = Random.Range(-1.5f, 1.5f);
            Camera camara = Camera.main;
            if (camara != null)
            {
                float profundidad = Mathf.Abs(camara.transform.position.z);
                float abajo = camara.ViewportToWorldPoint(new Vector3(0.5f, 0f, profundidad)).y + 1.3f;
                float arriba = camara.ViewportToWorldPoint(new Vector3(0.5f, 1f, profundidad)).y - 1.3f;
                if (arriba > abajo) altura = Mathf.Clamp(altura, abajo, arriba);
            }
            Vector3 lugar = new Vector3(origenX + VelocidadActual * tiempoGeneracion * 0.5f, altura, 0f);
            // Keep a player-sized approach zone clear, not just the coin's small trigger.
            foreach (Collider2D nearby in Physics2D.OverlapCircleAll(lugar, 1.35f))
                if (nearby.GetComponent<Obstaculo>() != null) return;
            Moneda moneda = Instantiate(monedaPrefab, lugar, Quaternion.identity);
            moneda.velocidad = VelocidadActual;
        }
    }

    public void ActualizarPuntos(int puntos)
    {
        puntosActuales = puntos;
    }
}
