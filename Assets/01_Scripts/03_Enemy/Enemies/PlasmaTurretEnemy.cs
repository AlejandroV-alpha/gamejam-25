using UnityEngine;

/// <summary>
/// Torreta de plasma fija que dispara 3 balas en forma de abanico.
/// Se sobrecalienta tras un numero de rafagas y se enfria despues de un tiempo.
/// Apunta al jugador al entrar en rango de alerta y dispara al entrar en rango de ataque.
/// </summary>
[RequireComponent(typeof(RangedAttackTriple))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class PlasmaTurretEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] LayerMask playerLayer;
    #endregion

    #region Fields
    RangedAttackTriple rangedAttack;
    RotatorTowardsTarget rotatorTowardsTarget;
    Transform currentTarget;
    float playerDistance;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        rangedAttack = GetComponent<RangedAttackTriple>();
        rotatorTowardsTarget = GetComponent<RotatorTowardsTarget>();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento en estado Idle: detecta jugador y cambia a alerta si es necesario.
    /// </summary>
    protected override void IdleBehaviour()
    {
        UpdateDetection();

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
        UpdateDetection();

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
    /// Considera sobrecalentamiento.
    /// </summary>
    protected override void AttackBehaviour()
    {
        UpdateDetection();

        if (currentTarget != null)
        {
            UpdateRotation();

            if (playerDistance <= attackRange)
            {
                if (rangedAttack.CanShoot())
                {
                    rangedAttack.Shoot();
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
    /// Detecta al jugador dentro del rango de alerta usando un solo OverlapCircle y calcula distancia.
    /// </summary>
    void UpdateDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, alertRange, playerLayer);

        if (hit != null)
        {
            currentTarget = hit.transform;
            playerDistance = Vector2.Distance(transform.position, currentTarget.position);
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
