using UnityEngine;

/// <summary>
/// Bala aliada que otorga energia a los objetivos que implementan ITakeEnergy.
/// </summary>
public class AllyEnergyBullet : BaseBullet
{
    #region Inspector Variables
    [Header("Energy Settings")]
    [SerializeField] float energyAmount = 20f;
    #endregion

    #region Override Methods
    /// <summary>
    /// Maneja el impacto de la bala contra un objeto del entorno.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeEnergy energyTarget))
        {
            energyTarget.TakeEnergy(energyAmount);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // Destroy the bullet
        Destroy(gameObject);
    }
    #endregion
}
