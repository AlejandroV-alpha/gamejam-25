using UnityEngine;

public class FreezeBullet : BaseBullet
{
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 0f;
    [SerializeField] float knockbackForce = 0f;

    [Header("Freeze Bullet Settings")]
    [SerializeField] float slowDuration = 2f;
    [SerializeField, Range(0f, 1f)] float speedMultiplier = 0.6f;

    protected override void HandleImpact(Collider2D collision)
    {
        if (damageAmount > 0f && collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        // Aplica ralentización si el objeto soporta ISlowable
        if (collision.TryGetComponent(out ISlowable slowable))
        {
            slowable.Slow(slowDuration, speedMultiplier);
        }

        // Efecto visual genérico
        SpawnImpactEffect();

        // Destruye la bala
        Destroy(gameObject);
    }
}
