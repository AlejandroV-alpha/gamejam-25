using UnityEngine;

/// <summary>
/// Bala comun del jugador. Causa daño a enemigos y se destruye al impactar.
/// </summary>
public class CommonBullet : BaseBullet
{
    #region Collision Behavior
    /// <summary>
    /// Logica personalizada de impacto para la bala comun.
    /// </summary>
    /// <param name="collision">Collider con el que impacto</param>
    protected override void HandleImpact(Collider2D collision)
    {
        Destroy(gameObject);
    }
    #endregion
}
