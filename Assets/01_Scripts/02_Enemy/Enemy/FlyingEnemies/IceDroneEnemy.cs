using UnityEngine;

/// <summary>
/// Enemigo aéreo tipo dron de hielo que patrulla una zona y cambia su comportamiento según su nivel de salud.
/// En la primera fase (salud alta), sigue al jugador con disparos simples.
/// Al caer por debajo del umbral de salud configurado, entra en una segunda fase donde incrementa su velocidad
/// y utiliza un disparo triple más agresivo. Se destruye al morir.
/// Incluye hélices giratorias y retroceso de torreta al disparar.
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

    [Header("Visual Parts")]
    [SerializeField] Transform torretaHielo;
    [SerializeField] Transform[] helices;
    [SerializeField] float velocidadHelices = 360f;
    [SerializeField] float retrocesoDistancia = 0.15f;
    [SerializeField] float retrocesoVelocidad = 8f;
    #endregion

    #region Protected Fields
    protected IShooter activeShooter;
    protected float currentFollowSpeed;
    protected bool isPhaseLow = false;

    Vector3 torretaPosInicial;
    float retrocesoActual = 0f;
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

        if (torretaHielo != null)
            torretaPosInicial = torretaHielo.localPosition;
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
        CheckPhaseThreshold();
        RotarHelices();
        RestaurarRetroceso();
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
                AplicarRetrocesoTorreta();
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
        bool lowPhaseNow = currentHealth <= maxHealth * healthPhaseTrigger;
        if (lowPhaseNow != isPhaseLow)
        {
            isPhaseLow = lowPhaseNow;
            activeShooter = isPhaseLow ? secondaryShooter : primaryShooter;
            currentFollowSpeed = isPhaseLow ? followSpeedPhase2 : followSpeed;
        }
    }
    #endregion

    #region Visual Animations
    void RotarHelices()
    {
        if (helices == null || helices.Length == 0) return;
        foreach (var h in helices)
        {
            if (h != null)
                h.Rotate(Vector3.forward, velocidadHelices * Time.deltaTime);
        }
    }

    void AplicarRetrocesoTorreta()
    {
        if (torretaHielo == null) return;
        torretaHielo.localPosition = torretaPosInicial - new Vector3(retrocesoDistancia, 0, 0);
        retrocesoActual = retrocesoDistancia;
    }

    void RestaurarRetroceso()
    {
        if (torretaHielo == null || retrocesoActual <= 0f) return;

        retrocesoActual = Mathf.MoveTowards(retrocesoActual, 0f, retrocesoVelocidad * Time.deltaTime);
        torretaHielo.localPosition = Vector3.Lerp(torretaHielo.localPosition, torretaPosInicial, Time.deltaTime * retrocesoVelocidad);
    }
    #endregion

    #region Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        DrawFlyingRanges();
    }
    #endregion
}
