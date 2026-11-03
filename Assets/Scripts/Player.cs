

using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movimiento")]
    public float fuerzaVuelo = 5f;
    public float gravedad = 1.5f;

    [Header("Limites de pantalla")]
    public float margenSuperior = 0.3f;
    public float margenInferior = 0.3f;

    private Rigidbody2D rb;
    private Collider2D colliderJugador;

    private bool estaPresionando = false;
    private bool estaVivo = true;

    public bool EstaVivo => estaVivo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliderJugador = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        estaVivo = true;
        estaPresionando = false;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravedad;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTapEvent += Tap;
        }
    }

    void Update()
    {
        if (!estaVivo) return;

        if (estaPresionando)
        {
            rb.linearVelocity = new Vector2(0, fuerzaVuelo);
        }
    }

    void FixedUpdate()
    {
        if (!estaVivo || Camera.main == null) return;

        Camera camara = Camera.main;

        float distancia = Mathf.Abs(
            transform.position.z - camara.transform.position.z);

        // Limites visibles de la camara
        float limiteArriba =
            camara.ViewportToWorldPoint(
                new Vector3(0.5f, 1f, distancia)).y;

        float limiteAbajo =
            camara.ViewportToWorldPoint(
                new Vector3(0.5f, 0f, distancia)).y;

        // Comprobar el borde real del pajaro
        float bordeSuperior = colliderJugador != null
            ? colliderJugador.bounds.max.y
            : transform.position.y;

        float bordeInferior = colliderJugador != null
            ? colliderJugador.bounds.min.y
            : transform.position.y;

        if (bordeSuperior >= limiteArriba - margenSuperior ||
            bordeInferior <= limiteAbajo + margenInferior)
        {
            Morir();
        }
    }

    private void Tap(bool presionado)
    {
        if (!estaVivo) return;

        estaPresionando = presionado;

        if (presionado && SonidosJuego.Instance != null)
        {
            SonidosJuego.Instance.SonidoVuelo();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!estaVivo) return;

        if (collision.gameObject.CompareTag("Obstaculo"))
        {
            Morir();
        }
    }

    private void Morir()
    {
        if (!estaVivo) return;

        estaVivo = false;
        estaPresionando = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Debug.Log("GAME OVER");

        if (SonidosJuego.Instance != null)
        {
            SonidosJuego.Instance.SonidoDerrota();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Perder();
        }
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTapEvent -= Tap;
        }
    }
}





