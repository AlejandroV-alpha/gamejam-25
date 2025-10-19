using UnityEngine;

[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class IceDroneEnemy : FlyingEnemy
{
    #region Inspector Variables
    [SerializeField] IShooter primaryShooter;
    [SerializeField] IShooter secondaryShooter;
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
    protected override void IdleBehaviour()
    {
        MoveToTarget(patrolTarget, patrolSpeed);
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f ||
            Vector2.Distance(transform.position, patrolOrigin) > maxDistanceFromOrigin)
        {
            SetRandomPatrolTarget();
        }
    }

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

    protected override void DeathBehaviour()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Detection
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
    void CheckPhaseThreshold()
    {
        bool lowPhaseNow = currentHealth <= maxHealth * 0.5f;
        if (lowPhaseNow != isPhaseLow)
        {
            isPhaseLow = lowPhaseNow;
            activeShooter = isPhaseLow ? secondaryShooter : primaryShooter;
            currentFollowSpeed = isPhaseLow ? followSpeedPhase2 : followSpeed;
        }
    }
    #endregion

    #region Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
