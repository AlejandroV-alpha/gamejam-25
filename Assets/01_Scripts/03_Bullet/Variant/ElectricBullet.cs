using UnityEngine;

/// <summary>
/// Bala electrica que inflige daño, empuje y puede aturdir al objetivo.
/// </summary>
public class ElectricBullet : BaseBullet
{
    #region Inspector Variables
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    [Header("Electric Bullet Settings")]
    [SerializeField] float stunDuration = 1f;
    #endregion

    #region Override Methods
    /// <summary>
    /// Maneja el impacto de la bala electrica contra un objeto del entorno.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        // Apply brief stun effect if supported
        if (collision.TryGetComponent(out IStunnable stunnable))
        {
            stunnable.Stun(stunDuration);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // Destroy the bullet
        Destroy(gameObject);
    }
    #endregion
}
