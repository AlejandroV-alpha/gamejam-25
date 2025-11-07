using UnityEngine;

/// <summary>
/// Dron aéreo de soporte que patrulla y cura aliados cercanos.
/// </summary>
[RequireComponent(typeof(RotatorTowardsTarget))]
[RequireComponent(typeof(IShooter))]
public class HealingDroneEnemy : FlyingEnemy
{
    #region Inspector Variables
    [Header("Detection")]
    [SerializeField] private LayerMask allyLayer;

    [Header("Visual / Anim")]
    [Tooltip("Contenedor visual (p.ej. 'Graphics'). Debe mover todo el dron.")]
    [SerializeField] Transform visualChild;
    [SerializeField] ProceduralDroneAnimator procAnim; // asocia el component del visualChild
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

        // auto-resolve refs
        if (!procAnim && visualChild)
            procAnim = visualChild.GetComponent<ProceduralDroneAnimator>();
        if (!visualChild && procAnim)
            visualChild = procAnim.transform;

        // estado visual inicial
        procAnim?.EnableHover(true);
        procAnim?.SetAlert(false);
        procAnim?.SetRotorsActive(true);
    }

    protected override void Update()
    {
        base.Update();
    }
    #endregion

    #region Target-State Logic
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
    protected override void IdleBehaviour()
    {
        // Visual: hover ON, alerta OFF
        procAnim?.EnableHover(true);
        procAnim?.SetAlert(false);
        procAnim?.SetRotorsActive(true);

        // Patrulla
        MoveToTarget(patrolTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f ||
            Vector2.Distance(transform.position, patrolOrigin) > maxDistanceFromOrigin)
        {
            SetRandomPatrolTarget();
        }
    }

    protected override void AlertBehaviour()
    {
        if (closestAllyTarget != null)
        {
            // Visual: hover ON, alerta ON
            procAnim?.EnableHover(true);
            procAnim?.SetAlert(true);
            procAnim?.SetRotorsActive(true);

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

    protected override void AttackBehaviour()
    {
        if (closestAllyTarget != null)
        {
            // Visual: hover ON, alerta ON (apuntando al aliado)
            procAnim?.EnableHover(true);
            procAnim?.SetAlert(true);
            procAnim?.SetRotorsActive(true);

            if (closestAllyDistance <= attackRange && shooter != null && shooter.CanShoot())
            {
                shooter.Shoot();

                // Recoil opuesto a la dirección de avance.
                // Si tu frente es UP local, usa Vector3.up * -1f
                Vector3 localBack = Vector3.up * -1f;
                procAnim?.PlayShootRecoil(localBack);
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
        // Apagado y caída. Destroy al terminar.
        if (procAnim != null) procAnim.PlayDeath(() => Destroy(gameObject));
        else Destroy(gameObject);
    }
    #endregion

    #region Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
