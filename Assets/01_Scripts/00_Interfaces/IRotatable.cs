using UnityEngine;

/// <summary>
/// Contrato para objetos que pueden rotar hacia un objetivo.
/// </summary>
public interface IRotatable
{
    /// <summary>
    /// Rota hacia el objetivo especificado.
    /// </summary>
    /// <param name="target">Transform del objetivo.</param>
    void RotateTowards(Transform target);
}
