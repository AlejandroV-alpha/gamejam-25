using UnityEngine;

/// <summary>
/// Enemigo aereo tipo dron centinela que patrulla una zona determinada.
/// Al detectar al jugador dentro de su rango de alerta, lo persigue a velocidad constante
/// y dispara proyectiles mientras permanece dentro del rango de ataque.
/// Si el jugador sale del area de deteccion, el dron regresa a su patrulla.
/// Se destruye al morir.
/// </summary>
[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class SentryDroneEnemy : FlyingEnemy
{
    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento de patrulla cuando el enemigo esta inactivo.
    /// </summary>
    protected override void IdleBehaviour()
    {
        MoveToTarget(patrolTarget, patrolSpeed);
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f || Vector2.Distance(transform.position, patrolOrigin) > maxDistanceFromOrigin)
        {
            SetRandomPatrolTarget();
        }
    }

    /// <summary>
    /// Comportamiento de alerta cuando el enemigo detecta un objetivo.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (currentTarget != null)
        {
            MoveToTarget(currentTarget.position, followSpeed);

            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange)
            {
                stateMachine.ChangeState(EnemyState.Attack);
            }
            else if (distance > alertRange)
            {
                currentTarget = null;
                rotator.ClearTarget();
                stateMachine.ChangeState(EnemyState.Idle);
            }
        }
        else
        {
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento de ataque cuando el enemigo esta en rango del objetivo.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange && shooter != null && shooter.CanShoot())
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
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento al morir, destruye el objeto del enemigo.
    /// </summary>
    protected override void DeathBehaviour()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Detection
    /// <summary>
    /// Detecta al jugador dentro del rango de alerta y actualiza el objetivo actual.
    /// </summary>
    protected override void UpdateDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, alertRange, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            currentTarget = hit.transform;
            rotator.SetTarget(currentTarget);
        }
        else
        {
            currentTarget = null;
            rotator.ClearTarget();
        }
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos visuales de deteccion y patrulla en la escena.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
