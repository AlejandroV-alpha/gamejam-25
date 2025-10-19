using UnityEngine;

public class SimpleBullet : BaseBullet
{
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        Destroy(gameObject);
    }
}
