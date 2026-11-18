using DG.Tweening;
using UnityEngine;
using Dino.Utility.Audio;

[RequireComponent(typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Moneda : MonoBehaviour
{
    public float velocidad = 3f;
    private bool recogida;
    private Camera camara;
    private SpriteRenderer visual;

    void Awake()
    {
        camara = Camera.main;
        visual = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (recogida || GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
        transform.Translate(Vector3.left * velocidad * Time.deltaTime, Space.World);
        if (camara != null && camara.WorldToViewportPoint(transform.position).x < -0.15f)
            Destroy(gameObject);
        else if (camara == null && transform.position.x < -20f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Player jugador = other.GetComponentInParent<Player>();
        if (jugador != null) IntentarRecoger(jugador);
    }

    public bool IntentarRecoger(Player jugador)
    {
        if (recogida || jugador == null || !jugador.EstaVivo || MetricasPartida.Instance == null ||
            GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return false;
        recogida = true;
        GetComponent<CircleCollider2D>().enabled = false;
        MetricasPartida.Instance.RecogerMoneda();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound("Moneda");
        transform.DOMoveY(transform.position.y + 0.7f, 0.25f).SetUpdate(true);
        transform.DOScale(transform.localScale * 1.6f, 0.25f).SetUpdate(true);
        visual.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(() => Destroy(gameObject));
        return true;
    }

    void OnDestroy()
    {
        transform.DOKill();
        if (visual != null) visual.DOKill();
    }
}
