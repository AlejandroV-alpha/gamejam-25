using UnityEngine;

public class EnergyBullet : BaseBullet
{
    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeEnergy energyTarget))
        {
            energyTarget.TakeEnergy(energyCost);
        }

        // Efecto visual genérico
        SpawnImpactEffect();

        // Destruye la bala
        Destroy(gameObject);
    }
}
