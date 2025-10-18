using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara cuando este entra en su rango de ataque.
/// Se pone en alerta cuando el jugador entra en un rango mayor.
/// Usa IShooter para manejar disparos y respeta obstaculos.
/// </summary>
[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class SimpleTurretEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] LayerMask playerLayer;

    [Header("Obstacle Settings")]
    [SerializeField] LayerMask obstacleLayer;
    #endregion

    #region Private Fields
    IShooter shooter;
    RotatorTowardsTarget rotatorTowardsTarget;
    Transform currentTarget;
    float playerDistance;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<IShooter>();
        rotatorTowardsTarget = GetComponent<RotatorTowardsTarget>();
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
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

            if (playerDistance <= attackRange)
            {
                if (shooter != null && shooter.CanShoot())
                {
                    shooter.Shoot();
                }
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

    #region Detection y Rotation
    /// <summary>
    /// Detecta al jugador dentro del rango de alerta, actualiza currentTarget y playerDistance.
    /// Considera obstáculos que bloqueen la visión.
    /// </summary>
    void UpdateDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, alertRange, playerLayer);

        if (hit != null)
        {
            Vector2 direction = (hit.transform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, hit.transform.position);

            // Raycast para detectar si hay obstáculos
            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer | playerLayer);
            if (rayHit.collider != null && ((1 << rayHit.collider.gameObject.layer) & playerLayer) != 0)
            {
                // Jugador visible
                currentTarget = hit.transform;
                playerDistance = distance;
                UpdateRotation();
            }
            else
            {
                // Jugador bloqueado por obstáculo
                currentTarget = null;
                playerDistance = 0f;
                ClearRotation();
            }
        }
        else
        {
            currentTarget = null;
            playerDistance = 0f;
            ClearRotation();
        }
    }

    /// <summary>
    /// Actualiza la rotacion hacia el jugador.
    /// </summary>
    void UpdateRotation()
    {
        if (currentTarget != null)
        {
            rotatorTowardsTarget.SetTarget(currentTarget);
        }
    }

    /// <summary>
    /// Limpia la rotacion si no hay jugador.
    /// </summary>
    void ClearRotation()
    {
        rotatorTowardsTarget.ClearTarget();
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos de alerta y ataque en el editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}
