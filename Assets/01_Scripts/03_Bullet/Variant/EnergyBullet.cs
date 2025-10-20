using UnityEngine;

/// <summary>
/// Bala de energia que restaura o transfiere energia al objetivo impactado.
/// </summary>
public class EnergyBullet : BaseBullet
{
    #region Override Methods
    /// <summary>
    /// Maneja el impacto de la bala de energia contra un objeto del entorno.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeEnergy energyTarget))
        {
            energyTarget.TakeEnergy(energyCost);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // Destroy the bullet
        Destroy(gameObject);
    }
    #endregion
}
