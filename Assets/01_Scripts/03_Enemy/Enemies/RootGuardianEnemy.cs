using UnityEngine;

/// <summary>
/// Enemigo fijo tipo Guardian Raiz.
/// Se oculta visualmente en Idle, emerge en Alert y dispara rafagas circulares en Attack.
/// Apunta al jugador mientras esta emergido.
/// </summary>
[RequireComponent(typeof(RangedAttackCircular))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class RootGuardianEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] LayerMask playerLayer;

    [Header("Visual Settings")]
    [SerializeField] Transform visualChild; // Asignar el sprite square o prefab visual
    #endregion

    #region Fields
    RangedAttackCircular rangedAttack;
    RotatorTowardsTarget rotatorTowardsTarget;
    Transform currentTarget;
    float playerDistance;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        rangedAttack = GetComponent<RangedAttackCircular>();
        rotatorTowardsTarget = GetComponent<RotatorTowardsTarget>();
        HideVisuals(); // Inicialmente oculto visual
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
            ShowVisuals(); // Emerger
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
            HideVisuals(); // Ocultarse
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
    /// Detecta al jugador dentro del rango de alerta y calcula distancia.
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

    #region Visual Control
    /// <summary>
    /// Activa la visibilidad del enemigo.
    /// Aqui se puede agregar trigger de Animator para animacion de "emergir"
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
    /// Aqui se puede agregar trigger de Animator para animacion de "ocultarse"
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
