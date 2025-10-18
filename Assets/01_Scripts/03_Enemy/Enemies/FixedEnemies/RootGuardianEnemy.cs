using UnityEngine;

[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class RootGuardianEnemy : FixedEnemy
{
    #region Inspector Variables
    [Header("Visual Settings")]
    [SerializeField] Transform visualChild;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        HideVisuals();
    }

    protected override void Update()
    {
        base.Update();
    }

    void OnDrawGizmosSelected()
    {
        DrawRanges();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento en estado Idle: oculta al enemigo y cambia a alerta si hay jugador.
    /// </summary>
    protected override void IdleBehaviour()
    {
        HideVisuals();

        if (currentTarget != null)
        {
            ShowVisuals();
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Comportamiento en estado Alert: rota hacia el jugador y cambia a ataque si esta en rango.
    /// Si el jugador deja de ser visible, el enemigo se oculta y vuelve a Idle.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (currentTarget != null)
        {
            ShowVisuals();
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
    /// Comportamiento en estado Attack: dispara si el jugador esta en rango.
    /// Si el jugador deja de ser visible o no hay linea de vision, el enemigo se oculta.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            ShowVisuals();
            UpdateRotation();

            if (playerDistance <= attackRange && shooter.CanShoot())
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

    #region Visual Control
    /// <summary>
    /// Muestra las partes visibles del enemigo.
    /// </summary>
    void ShowVisuals()
    {
        if (visualChild != null && !visualChild.gameObject.activeSelf)
        {
            visualChild.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta las partes visibles del enemigo.
    /// </summary>
    void HideVisuals()
    {
        if (visualChild != null && visualChild.gameObject.activeSelf)
        {
            visualChild.gameObject.SetActive(false);
        }
    }
    #endregion
}
