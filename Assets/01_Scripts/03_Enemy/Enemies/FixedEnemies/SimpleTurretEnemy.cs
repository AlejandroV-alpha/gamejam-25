using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara cuando este entra en su rango de ataque.
/// Se pone en alerta cuando el jugador entra en un rango mayor.
/// Usa IShooter para manejar disparos y respeta obstaculos.
/// </summary>
public class SimpleTurretEnemy : FixedEnemy
{
    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    void OnDrawGizmosSelected()
    {
        DrawRanges();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento en estado Idle: detecta jugador y cambia a alerta si es necesario.
    /// </summary>
    protected override void IdleBehaviour()
    {
        if (currentTarget != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Comportamiento en estado Alert: apunta al jugador y cambia a ataque si esta en rango.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (currentTarget != null)
        {
            UpdateRotation();
            if (playerDistance <= attackRange)
            {
                stateMachine.ChangeState(EnemyState.Attack);
            }
        }
        else
        {
            ClearRotation();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento en estado Attack: dispara al jugador si esta en rango.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            UpdateRotation();
            if (playerDistance <= attackRange && shooter != null && shooter.CanShoot())
            {
                shooter.Shoot();
            }
            else
            {
                stateMachine.ChangeState(EnemyState.Alert);
            }
        }
        else
        {
            ClearRotation();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento en estado Death: destruye el enemigo.
    /// </summary>
    protected override void DeathBehaviour()
    {
        Destroy(gameObject);
    }
    #endregion
}
