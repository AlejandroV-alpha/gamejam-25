using UnityEngine;

public class ElectricBullet : BaseBullet
{
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    [Header("Electric Bullet Settings")]
    [SerializeField] float stunDuration = 1f;

    protected override void HandleImpact(Collider2D collision)
    {
        // Aplica daño
        if (collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        // Aplica efecto de aturdimiento breve si el jugador lo soporta
        if (collision.TryGetComponent(out IStunnable stunnable))
        {
            stunnable.Stun(stunDuration);
        }

        SpawnImpactEffect();
        Destroy(gameObject);
    }
}
