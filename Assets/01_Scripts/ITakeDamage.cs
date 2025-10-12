using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden recibir daño.
/// Incluye knockback opcional para efectos de impacto.
/// </summary>
public interface ITakeDamage
{
    void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce = 0f);
}
