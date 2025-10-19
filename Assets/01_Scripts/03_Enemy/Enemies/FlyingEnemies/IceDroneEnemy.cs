using UnityEngine;

/// <summary>
/// Enemigo aereo tipo dron de hielo que patrulla una zona y cambia su comportamiento segun su nivel de salud.
/// En la primera fase (salud alta), sigue al jugador con disparos simples.
/// Al caer por debajo del umbral de salud configurado, entra en una segunda fase donde incrementa su velocidad
/// y utiliza un disparo triple mas agresivo. Se destruye al morir.
/// </summary>
[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class IceDroneEnemy : FlyingEnemy
{
    #region Inspector Variables
    [SerializeField] IShooter primaryShooter;
    [SerializeField] IShooter secondaryShooter;

    [Header("Phase Settings")]
    [SerializeField, Range(0f, 1f)] float healthPhaseTrigger = 0.5f;
    [SerializeField] float followSpeedPhase2 = 4f;
    #endregion

    #region Protected Fields
    protected IShooter activeShooter;
    protected float currentFollowSpeed;
    protected bool isPhaseLow = false;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        rotator = GetComponent<RotatorTowardsTarget>();
        primaryShooter = GetComponent<RangedAttack>();
        secondaryShooter = GetComponent<RangedAttackTriple>();
        activeShooter = primaryShooter;
        currentFollowSpeed = followSpeed;
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
        CheckPhaseThreshold();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento de patrulla cuando el dron esta inactivo.
    /// </summary>
    protected override void IdleBehaviour()
    {
        MoveToTarget(patrolTarget, patrolSpeed);
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f ||
            Vector2.Distance(transform.position, patrolOrigin) > maxDistanceFromOrigin)
        {
            SetRandomPatrolTarget();
        }
    }

    /// <summary>
    /// Comportamiento de seguimiento cuando el dron detecta un objetivo.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (currentTarget != null)
        {
            MoveToTarget(currentTarget.position, currentFollowSpeed);
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
    /// Comportamiento de ataque, dispara al objetivo dependiendo de la fase actual.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= attackRange && activeShooter != null && activeShooter.CanShoot())
            {
                activeShooter.Shoot();
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

    #region Phase Logic
    /// <summary>
    /// Verifica si la salud ha bajado del 50% para cambiar de fase y ajustar velocidad y disparo.
    /// </summary>
    void CheckPhaseThreshold()
    {
        bool lowPhaseNow = currentHealth <= maxHealth * healthPhaseTrigger;
        if (lowPhaseNow != isPhaseLow)
        {
            isPhaseLow = lowPhaseNow;
            activeShooter = isPhaseLow ? secondaryShooter : primaryShooter;
            currentFollowSpeed = isPhaseLow ? followSpeedPhase2 : followSpeed;
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
