using UnityEngine;

/// <summary>
/// Bala de hielo que causa daño opcional y aplica un efecto de ralentizacion
/// a los objetivos que implementan la interfaz ISlowable.
/// </summary>
public class FreezeBullet : BaseBullet
{
    #region Inspector Variables
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 0f;
    [SerializeField] float knockbackForce = 0f;

    [Header("Freeze Bullet Settings")]
    [SerializeField] float slowDuration = 2f;
    [SerializeField, Range(0f, 1f)] float speedMultiplier = 0.6f;
    #endregion

    #region Override Methods
    /// <summary>
    /// Maneja el impacto de la bala de hielo contra un objeto del entorno.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        if (damageAmount > 0f && collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        // Apply slow effect if supported
        if (collision.TryGetComponent(out ISlowable slowable))
        {
            slowable.Slow(slowDuration, speedMultiplier);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // Destroy the bullet
        Destroy(gameObject);
    }
    #endregion
}
