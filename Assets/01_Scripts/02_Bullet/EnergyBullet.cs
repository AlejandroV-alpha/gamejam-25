using UnityEngine;

/// <summary>
/// Bala de energia. Transfiere energia a objetos que implementen ITakeEnergy.
/// </summary>
public class EnergyBullet : BaseBullet
{
    [Header("Energy Settings")]
    [SerializeField] float energyAmount = 5f;

    #region Collision Behavior
    /// <summary>
    /// Logica personalizada al impactar: transfiere energia a objetos que implementen ITakeEnergy.
    /// </summary>
    /// <param name="collision">Collider con el que impacto la bala</param>
    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeEnergy energyTarget))
        {
            energyTarget.ReceiveEnergy(energyAmount);
        }

        Destroy(gameObject);
    }
    #endregion

    #region Utilities
    public override float GetEnergyCost()
    {
        return energyAmount;
    }
    #endregion

}
