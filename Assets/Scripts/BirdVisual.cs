using DG.Tweening;
using UnityEngine;

/// <summary>Presentation only: never rotates or scales the physics body.</summary>
public class BirdVisual : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Animator animator;
    public ParticleSystem deathEffect;
    private Rigidbody2D body;
    private Player player;

    void Awake() { body = GetComponent<Rigidbody2D>(); player = GetComponent<Player>(); }
    void OnEnable()
    {
        if (sprite == null || animator == null) return;
        sprite.transform.DOKill(); sprite.DOKill();
        sprite.transform.localRotation = Quaternion.identity;
        sprite.transform.localScale = Vector3.one;
        sprite.color = Color.white;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.SetBool("Dead", false);
        animator.Play("Flight", 0, 0);
    }
    void LateUpdate()
    {
        if (body == null || player == null || !player.EstaVivo || sprite == null) return;
        float angle = Mathf.Clamp(body.linearVelocity.y * 4f, -28f, 18f);
        sprite.transform.localRotation = Quaternion.Lerp(sprite.transform.localRotation,
            Quaternion.Euler(0, 0, angle), 1f - Mathf.Exp(-9f * Time.deltaTime));
        animator.speed = body.linearVelocity.y > 0 ? 1.15f : 0.85f;
    }
    public void Die()
    {
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (PlayerPrefs.GetInt("Vibracion", 1) != 0) Handheld.Vibrate();
#endif
        if (sprite == null || animator == null) return;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.SetBool("Dead", true);
        sprite.DOColor(new Color(1f, 0.45f, 0.35f), 0.08f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        sprite.transform.DOLocalRotate(new Vector3(0, 0, -95), 0.42f).SetEase(Ease.InQuad).SetUpdate(true);
        sprite.transform.DOScale(0.55f, 0.42f).SetEase(Ease.InBack).SetUpdate(true);
    }
    void OnDisable()
    {
        if (sprite == null) return;
        sprite.transform.DOKill(); sprite.DOKill();
    }
}
