using UnityEngine;

public class CoinVisual : MonoBehaviour
{
    public SpriteRenderer artwork;
    private Vector3 scale;
    private Moneda coin;
    private float phase;
    void Awake() { coin = GetComponent<Moneda>(); scale = artwork.transform.localScale; phase = Random.value * 6.28f; }
    void Update()
    {
        if (coin.Recogida || GameManager.Instance == null || !GameManager.Instance.PartidaActiva) return;
        phase += Time.deltaTime * 3;
        artwork.transform.localScale = new Vector3(scale.x * (0.35f + 0.65f * Mathf.Abs(Mathf.Cos(phase))), scale.y, scale.z);
        artwork.transform.localPosition = new Vector3(0, Mathf.Sin(phase * 0.75f) * 0.045f, 0);
        float glow = 0.92f + 0.08f * Mathf.Pow(Mathf.Max(0, Mathf.Cos(phase)), 8);
        artwork.color = new Color(1, glow, glow, 1);
    }
}
