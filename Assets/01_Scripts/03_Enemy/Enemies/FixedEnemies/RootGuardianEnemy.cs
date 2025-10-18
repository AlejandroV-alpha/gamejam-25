using UnityEngine;

/// <summary>
/// Enemigo fijo tipo Guardian Raiz.
/// Se oculta visualmente en Idle, emerge en Alert y dispara rafagas circulares en Attack.
/// Apunta al jugador mientras esta emergido.
/// Usa IShooter para manejar disparos y respeta obstaculos.
/// </summary>
[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class RootGuardianEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] LayerMask playerLayer;

    [Header("Obstacle Settings")]
    [SerializeField] LayerMask obstacleLayer;

    [Header("Visual Settings")]
    [SerializeField] Transform visualChild;
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
        HideVisuals();
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento en estado Idle: oculta el enemigo y detecta jugador.
    /// </summary>
    protected override void IdleBehaviour()
    {
        HideVisuals();
        UpdateDetection();
        if (currentTarget != null)
        {
            ShowVisuals();
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
            HideVisuals();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Comportamiento en estado Attack: dispara rafaga circular si jugador esta en rango.
    /// </summary>
    protected override void AttackBehaviour()
    {
        UpdateDetection();
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
            HideVisuals();
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

    #region Visual Control
    /// <summary>
    /// Activa la visibilidad del enemigo.
    /// </summary>
    void ShowVisuals()
    {
        if (visualChild != null)
        {
            visualChild.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta el enemigo.
    /// </summary>
    void HideVisuals()
    {
        if (visualChild != null)
        {
            visualChild.gameObject.SetActive(false);
        }
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
