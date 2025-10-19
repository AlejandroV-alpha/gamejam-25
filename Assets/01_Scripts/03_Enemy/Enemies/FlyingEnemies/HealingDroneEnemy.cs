using UnityEngine;

/// <summary>
/// Dron aereo de soporte que patrulla el area en busca de aliados danados.
/// Cuando detecta un aliado dentro de su rango de alerta, lo sigue y dispara proyectiles de curacion.
/// Mantiene una distancia prudente y prioriza al aliado mas cercano. Se destruye al morir.
/// </summary>
[RequireComponent(typeof(RotatorTowardsTarget))]
[RequireComponent(typeof(IShooter))]
public class HealingDroneEnemy : FlyingEnemy
{
    #region Inspector Variables
    [Header("Detection")]
    [SerializeField] private LayerMask allyLayer;
    #endregion

    #region Protected Fields
    protected Transform closestAllyTarget;
    protected float closestAllyDistance;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        patrolOrigin = transform.position;
        SetRandomPatrolTarget();
    }

    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Actualiza el estado del dron segun la distancia del aliado detectado.
    /// </summary>
    protected override void UpdateTargetState()
    {
        if (closestAllyTarget == null)
        {
            stateMachine.ChangeState(EnemyState.Idle);
            return;
        }

        if (closestAllyDistance <= attackRange)
        {
            stateMachine.ChangeState(EnemyState.Attack);
        }
        else if (closestAllyDistance <= alertRange)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
        else
        {
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }
    #endregion

    #region Detection
    /// <summary>
    /// Detecta aliados dentro del rango de alerta y selecciona el mas cercano.
    /// </summary>
    protected override void UpdateDetection()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alertRange, allyLayer);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = hit.transform;
            }
        }

        closestAllyTarget = closest;
        closestAllyDistance = minDistance;

        if (closestAllyTarget != null)
        {
            rotator.SetTarget(closestAllyTarget);
        }
        else
        {
            rotator.ClearTarget();
            closestAllyDistance = Mathf.Infinity;
        }
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento de patrulla cuando no hay aliados detectados.
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
    /// Comportamiento de seguimiento cuando hay un aliado dentro del rango de alerta.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (closestAllyTarget != null)
        {
            MoveToTarget(closestAllyTarget.position, followSpeed);

            if (closestAllyDistance <= attackRange)
            {
                stateMachine.ChangeState(EnemyState.Attack);
            }
            else if (closestAllyDistance > alertRange)
            {
                closestAllyTarget = null;
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
    /// Comportamiento de ataque, dispara balas de curacion al aliado mas cercano.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (closestAllyTarget != null)
        {
            if (closestAllyDistance <= attackRange && shooter != null && shooter.CanShoot())
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
    /// Comportamiento al morir, destruye el dron.
    /// </summary>
    protected override void DeathBehaviour()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos visuales del dron sanador en la escena.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
