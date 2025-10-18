using UnityEngine;

/// <summary>
/// Contrato general que define el comportamiento básico de cualquier enemigo.
/// Hereda de ITakeDamage para manejo de daño.
/// </summary>
public interface IEnemy : ITakeDamage
{
    Transform Target { get; set; }

    /// <summary>
    /// Inicializa el enemigo al comenzar.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Actualiza el comportamiento del enemigo por frame.
    /// </summary>
    void Tick();

    /// <summary>
    /// Se llama cuando el enemigo detecta al jugador.
    /// </summary>
    void OnDetectPlayer(Transform player);

    /// <summary>
    /// Se llama cuando el enemigo pierde al jugador de vista.
    /// </summary>
    void OnLosePlayer();

    /// <summary>
    /// Acciones al morir (animación, efectos, loot, etc.).
    /// </summary>
    void OnDeath();
}
