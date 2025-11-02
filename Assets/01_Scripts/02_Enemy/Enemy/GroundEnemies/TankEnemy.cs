using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemigo terrestre tipo tanque ligero que patrulla aleatoriamente dentro de un radio definido,
/// persigue al jugador al detectarlo y dispara cuando esta en rango de ataque.
/// Se destruye al morir y evita obstaculos definidos en la capa obstacleLayer al patrullar.
/// </summary>
[RequireComponent(typeof(RangedAttack))]
[RequireComponent(typeof(Collider2D))]
public class TankEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Patrol")]
    [SerializeField] float patrolRadius = 3f;
    [SerializeField] float patrolSpeed = 1.5f;
    [SerializeField] float patrolPointTolerance = 0.7f;
    [SerializeField] float minPatrolDistance = 0.5f;
    [SerializeField] float timeBetweenPatrolPoints = 3f;

    [Header("Obstacles")]
    [SerializeField] LayerMask obstacleLayer;

    [Header("Chase")]
    [SerializeField] float chaseSpeed = 2.5f;
    [SerializeField] float pathUpdateInterval = 1f;
    [SerializeField] float stuckTimeThreshold = 5f;
    [SerializeField] float stuckDistanceThreshold = 0.2f;

    [Header("Attack")]
    [SerializeField] float predictiveAiming = 0.3f;
    [SerializeField] float aimTolerance = 10f;
    #endregion

    #region Private Fields
    Vector2 originPosition;
    Vector2 patrolTarget;
    NavMeshAgent agent;
    IShooter ishooter;
    float lastPatrolTime;
    float lastPathUpdateTime;
    float stuckTimer;
    Vector2 lastPosition;
    bool hasValidPatrolTarget;
    bool isStuck;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        originPosition = transform.position;
        lastPosition = transform.position;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = patrolSpeed;
        agent.acceleration = 8f;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = 0.1f;

        ishooter = GetComponent<IShooter>();
    }

    protected override void Start()
    {
        base.Start();
        ChooseNewPatrolPoint();
        lastPatrolTime = Time.time;
        lastPathUpdateTime = Time.time;
    }

    protected override void Update()
    {
        if (!stateMachine.IsInState(EnemyState.Death))
        {
            agent.isStopped = false;
            CheckIfStuck();
        }

        base.Update();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento cuando el enemigo esta idle.
    /// Patrulla el area y cambia a alert si detecta al jugador.
    /// </summary>
    protected override void IdleBehaviour()
    {
        isStuck = false;

        if (!hasValidPatrolTarget || Time.time - lastPatrolTime > timeBetweenPatrolPoints || isStuck)
        {
            ChooseNewPatrolPoint();
            lastPatrolTime = Time.time;
            stuckTimer = 0f;
        }

        agent.speed = patrolSpeed;

        if (ReachedPatrolPoint() && Time.time - lastPatrolTime > 1f)
        {
            ChooseNewPatrolPoint();
            lastPatrolTime = Time.time;
        }

        if (Target != null)
        {
            if (Vector2.Distance(transform.position, Target.position) <= alertRange && HasLineOfSightToTarget())
            {
                stateMachine.ChangeState(EnemyState.Alert);
            }
        }
    }

    /// <summary>
    /// Comportamiento cuando el enemigo esta alert.
    /// Persigue al jugador y cambia a attack si esta en rango.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (Target == null)
        {
            stateMachine.ChangeState(EnemyState.Idle);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, Target.position);

        if (Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            agent.SetDestination(Target.position);
            lastPathUpdateTime = Time.time;
        }

        if (distanceToPlayer <= attackRange && HasLineOfSightToTarget())
        {
            stateMachine.ChangeState(EnemyState.Attack);
            return;
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;

        if (distanceToPlayer > alertRange || !HasLineOfSightToTarget())
        {
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento cuando el enemigo esta en ataque.
    /// Dispara al jugador si esta alineado y en rango.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (Target == null)
        {
            stateMachine.ChangeState(EnemyState.Idle);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, Target.position);

        if (!HasLineOfSightToTarget() || distanceToPlayer > attackRange)
        {
            stateMachine.ChangeState(EnemyState.Alert);
            return;
        }

        agent.isStopped = true;
        OrientWithPrediction();

        if (ishooter != null)
        {
            if (ishooter.CanShoot())
            {
                if (IsAimedAtTarget())
                {
                    ishooter.Shoot();
                }
            }
        }
    }

    /// <summary>
    /// Comportamiento cuando el enemigo muere.
    /// Detiene el agente y destruye el objeto.
    /// </summary>
    protected override void DeathBehaviour()
    {
        agent.isStopped = true;
        agent.velocity = Vector2.zero;
        Destroy(gameObject, 0.1f);
    }
    #endregion

    #region Patrol Logic
    /// <summary>
    /// Elige un nuevo punto de patrulla valido dentro del radio definido.
    /// Evita obstaculos y asegura que el punto sea alcanzable.
    /// </summary>
    void ChooseNewPatrolPoint()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * patrolRadius;
            Vector2 randomPoint = originPosition + new Vector2(randomDir.x, randomDir.y);

            if (Vector2.Distance(randomPoint, transform.position) < minPatrolDistance)
            {
                continue;
            }

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        if (!HasObstaclesBetween(transform.position, hit.position))
                        {
                            patrolTarget = hit.position;
                            agent.SetDestination(patrolTarget);
                            hasValidPatrolTarget = true;
                            stuckTimer = 0f;
                            return;
                        }
                    }
                }
            }
        }

        patrolTarget = originPosition;
        agent.SetDestination(patrolTarget);
        hasValidPatrolTarget = true;
    }

    /// <summary>
    /// Comprueba si el enemigo alcanzo el punto de patrulla.
    /// </summary>
    bool ReachedPatrolPoint()
    {
        if (!hasValidPatrolTarget)
        {
            return true;
        }

        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance + patrolPointTolerance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
    #endregion

    #region Movement & Detection
    /// <summary>
    /// Comprueba si el enemigo esta atascado mientras se mueve.
    /// </summary>
    void CheckIfStuck()
    {
        float distanceMoved = Vector2.Distance(transform.position, lastPosition);

        if (distanceMoved < stuckDistanceThreshold && agent.hasPath && agent.velocity.magnitude > 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeThreshold)
            {
                isStuck = true;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
            isStuck = false;
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// Comprueba si hay obstaculos entre dos puntos.
    /// </summary>
    bool HasObstaclesBetween(Vector2 from, Vector2 to)
    {
        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);
        RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, obstacleLayer);
        if (hit.collider != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Orienta al enemigo hacia la posicion predicha del objetivo.
    /// </summary>
    void OrientWithPrediction()
    {
        if (Target == null)
        {
            return;
        }

        Rigidbody2D targetRb = Target.GetComponent<Rigidbody2D>();
        Vector2 predictedPosition = (Vector2)Target.position;

        if (targetRb != null && targetRb.linearVelocity.magnitude > 0.1f)
        {
            float timeToReach = Vector2.Distance(transform.position, Target.position) / 10f;
            predictedPosition += targetRb.linearVelocity * timeToReach * predictiveAiming;
        }

        Vector2 direction = (predictedPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Comprueba si el enemigo esta correctamente apuntando al objetivo.
    /// </summary>
    bool IsAimedAtTarget()
    {
        if (Target == null)
        {
            return false;
        }

        float angleDifference = Vector2.Angle(transform.up, (Target.position - transform.position).normalized);
        if (angleDifference <= aimTolerance)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Comprueba si hay linea de vision directa al objetivo.
    /// </summary>
    bool HasLineOfSightToTarget()
    {
        if (Target == null)
        {
            return false;
        }

        Vector2 direction = (Target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, Target.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        if (hit.collider == null)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Aplica daño al enemigo y lo cambia a estado alert si detecta al jugador.
    /// </summary>
    public override void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce)
    {
        base.TakeDamage(damage, hitDirection, knockbackForce);

        if (!stateMachine.IsInState(EnemyState.Death) && Target != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Detecta al jugador y activa el agente.
    /// </summary>
    public override void OnDetectPlayer(Transform player)
    {
        base.OnDetectPlayer(player);
        agent.isStopped = false;
    }

    /// <summary>
    /// Actualiza el estado del objetivo segun distancia y linea de vision.
    /// </summary>
    protected override void UpdateTargetState()
    {
        if (Target == null || stateMachine.IsInState(EnemyState.Death))
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, Target.position);
        bool hasLOS = HasLineOfSightToTarget();

        if (!stateMachine.IsInState(EnemyState.Attack))
        {
            if (distance <= attackRange && hasLOS)
            {
                stateMachine.ChangeState(EnemyState.Attack);
            }
            else if (distance <= alertRange && hasLOS)
            {
                stateMachine.ChangeState(EnemyState.Alert);
            }
            else
            {
                stateMachine.ChangeState(EnemyState.Idle);
            }
        }
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos y puntos de patrulla para debugging.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        DrawRanges();

        Vector2 center = originPosition == Vector2.zero ? transform.position : originPosition;

        // Radio de patrulla
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, patrolRadius);

        // Punto de patrulla actual
        if (Application.isPlaying && hasValidPatrolTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(patrolTarget, 0.3f);
            Gizmos.DrawLine(transform.position, patrolTarget);
        }
    }
    #endregion
}
