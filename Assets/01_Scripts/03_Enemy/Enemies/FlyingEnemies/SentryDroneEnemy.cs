using UnityEngine;

[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class SentryDroneEnemy : FlyingEnemy
{
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

    #region Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
