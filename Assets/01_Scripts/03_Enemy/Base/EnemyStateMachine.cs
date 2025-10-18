using UnityEngine;

/// <summary>
/// Controla el estado actual del enemigo y las transiciones entre estados.
/// </summary>
public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    /// <summary>
    /// Cambia al nuevo estado si es distinto al actual.
    /// </summary>
    public void ChangeState(EnemyState newState)
    {
        if (newState != CurrentState)
        {
            CurrentState = newState;
        }  
    }

    /// <summary>
    /// Verifica si el enemigo está en cierto estado.
    /// </summary>
    public bool IsInState(EnemyState state)
    {
        return CurrentState == state;
    }
}

/// <summary>
/// Estados posibles de un enemigo.
/// </summary>
public enum EnemyState
{
    Idle,
    Alert,
    Attack,
    Death
}
