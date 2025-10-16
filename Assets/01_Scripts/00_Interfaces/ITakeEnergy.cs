using UnityEngine;

/// <summary>
/// Interfaz para objetos que pueden recibir energía de balas u otras fuentes.
/// Implementa este método para definir cómo el objeto procesa la energía recibida.
/// </summary>
public interface ITakeEnergy
{
    /// <summary>
    /// Recibe una cantidad de energía y la aplica al objeto según su lógica interna.
    /// </summary>
    /// <param name="amount">Cantidad de energía a recibir.</param>
    void ReceiveEnergy(float amount);
}