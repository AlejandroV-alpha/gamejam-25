using UnityEngine;

[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public class RootGuardianEnemy : FixedEnemy
{
    #region Inspector Variables
    [Header("Visual Settings")]
    [SerializeField] Transform visualChild;

    [Header("Procedural Anim (sin pulso ni flash de daño)")]
    [SerializeField] ProceduralEnemyAnimator procAnim; // Asignar el componente del visualChild
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        HideVisuals();

        if (!procAnim && visualChild)
            procAnim = visualChild.GetComponent<ProceduralEnemyAnimator>();
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
    /// Idle: oculto. Si hay jugador, emerge y pasa a Alert.
    /// </summary>
    protected override void IdleBehaviour()
    {
        HideVisuals();

        if (currentTarget != null)
        {
            ShowVisuals();
            procAnim?.PlayEmerge();
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Alert: rota hacia el jugador, si entra en rango Attack. Si pierde jugador  ocultarse.
    /// </summary>
    protected override void AlertBehaviour()
    {
        if (currentTarget != null)
        {
            ShowVisuals();
            UpdateRotation(); // tu rotación hacia el objetivo

            if (playerDistance <= attackRange)
            {
                stateMachine.ChangeState(EnemyState.Attack);
            }
        }
        else
        {
            // Oculta con animación y desactiva al terminar
            procAnim?.PlayHide(() => { HideVisuals(); });
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Attack: rota y dispara. Si sale de rango/visión  Alert.
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

                // Recoil en local hacia atrás. Asumiendo que “arriba” (up) del sprite es hacia delante.
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
            procAnim?.PlayHide(() => { HideVisuals(); });
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// Muerte: colapso y destroy.
    /// </summary>
    protected override void DeathBehaviour()
    {
        if (procAnim != null)
        {
            procAnim.PlayDeath(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Visual Control
    /// <summary>
    /// Muestra las partes visibles del enemigo.
    /// </summary>
    void ShowVisuals()
    {
        if (visualChild != null && !visualChild.gameObject.activeSelf)
            visualChild.gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta las partes visibles del enemigo.
    /// </summary>
    void HideVisuals()
    {
        if (visualChild != null && visualChild.gameObject.activeSelf)
            visualChild.gameObject.SetActive(false);
    }
    #endregion

    // (Opcional) Forzar “nudge” manual si te sirve:
    public void NudgeAimLeft() => procAnim?.PlayAimNudge(-1f);
    public void NudgeAimRight() => procAnim?.PlayAimNudge(+1f);
}
