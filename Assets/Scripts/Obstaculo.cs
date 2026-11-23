

using UnityEngine;

public class Obstaculo : MonoBehaviour
{
    public float velocidad = 3f;

    private Player jugador;
    private Collider2D colliderObstaculo;
    private Collider2D colliderJugador;

    private bool puntoSumado = false;

    void Start()
    {
        jugador = FindFirstObjectByType<Player>();

        colliderObstaculo = GetComponent<Collider2D>();

        if (jugador != null)
        {
            colliderJugador = jugador.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
        // Movimiento del obstaculo
        transform.Translate(
            Vector2.left * velocidad * Time.deltaTime
        );

        // Detectar cuando el jugador supera el obstaculo
        if (!puntoSumado &&
            jugador != null &&
            jugador.EstaVivo &&
            colliderObstaculo != null &&
            colliderJugador != null)
        {
            if (colliderObstaculo.bounds.max.x <
                colliderJugador.bounds.min.x)
            {
                puntoSumado = true;

                if (Puntuacion.Instance != null)
                {
                    Puntuacion.Instance.SumarPunto();
                }

                if (SonidosJuego.Instance != null)
                {
                    SonidosJuego.Instance.SonidoPunto();
                }
            }
        }

        // Eliminar obstaculos fuera de pantalla
        float limite = Camera.main != null ? Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect - 2f : -12f;
        if (transform.position.x < limite)
        {
            Destroy(gameObject);
        }
    }
}


