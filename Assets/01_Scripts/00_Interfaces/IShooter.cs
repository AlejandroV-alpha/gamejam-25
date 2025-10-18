using UnityEngine;

/// <summary>
/// Contrato para enemigos que pueden disparar proyectiles.
/// </summary>
public interface IShooter
{
    /// <summary>
    /// Dispara un proyectil o realiza ataque a distancia.
    /// </summary>
    void Shoot();
}
