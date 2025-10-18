using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara rafagas de balas.
/// Usa IShooter para disparos y RotatorTowardsTarget para apuntar.
/// </summary>
[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class IceSentinelEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] LayerMask playerLayer;
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
    /// Comportamiento en estado Idle: sin jugador detectado, mantiene rotacion inicial.
    /// </summary>
    protected override void IdleBehaviour()
    {
        if (currentTarget != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Comportamiento en estado Alert: rota hacia el jugador y cambia a ataque si está en rango.
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
    /// Comportamiento en estado Attack: dispara rafagas si el jugador esta en rango.
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
    /// Detecta al jugador dentro del rango de alerta y calcula distancia.
    /// </summary>
    void UpdateDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, alertRange, playerLayer);
        if (hit != null)
        {
            currentTarget = hit.transform;
            playerDistance = Vector2.Distance(transform.position, currentTarget.position);
            UpdateRotation();
        }
        else
        {
            currentTarget = null;
            playerDistance = 0f;
            ClearRotation();
        }
    }

    /// <summary>
    /// Actualiza la rotación hacia el jugador.
    /// </summary>
    void UpdateRotation()
    {
        if (currentTarget != null)
        {
            rotatorTowardsTarget.SetTarget(currentTarget);
        }
    }

    /// <summary>
    /// Limpia la rotación si no hay jugador detectado.
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
