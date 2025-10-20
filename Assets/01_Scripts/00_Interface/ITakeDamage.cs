using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden recibir daño de balas, ataques u otras fuentes.
/// Incluye la posibilidad de aplicar un empuje (knockback) en la dirección del impacto.
/// </summary>
public interface ITakeDamage
{
    /// <summary>
    /// Aplica daño al objeto y opcionalmente un empuje en la dirección del impacto.
    /// </summary>
    /// <param name="damage">Cantidad de daño a aplicar.</param>
    /// <param name="hitDirection">Dirección desde la que proviene el impacto.</param>
    /// <param name="knockbackForce">Fuerza del empuje aplicado al objeto (opcional).</param>
    void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce);
}
