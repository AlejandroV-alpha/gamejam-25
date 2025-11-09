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

    #region Audio (solo DISPARO)
    [Header("SFX")]
    [Tooltip("Se crea automáticamente si no existe.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Clip al DISPARAR (p.ej. BalaHieloDron).")]
    [SerializeField] private AudioClip shootSfx;

    [Range(0f, 1f)][SerializeField] private float shootVolume = 0.9f;
    [SerializeField] private Vector2 shootPitchRandom = new Vector2(0.97f, 1.03f);
    [Tooltip("0=2D, 1=3D. En 2D con espacialidad, deja ~0.7-1.")]
    [Range(0f, 1f)][SerializeField] private float spatialBlend = 1f;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        HideVisuals();

        if (!procAnim && visualChild)
            procAnim = visualChild.GetComponent<ProceduralEnemyAnimator>();

        // AudioSource seguro
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 15f;
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
    /// <summary>Idle: oculto. Si hay jugador, emerge y pasa a Alert.</summary>
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

    /// <summary>Alert: rota hacia el jugador, si entra en rango Attack. Si pierde jugador, ocultarse.</summary>
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
            StartHide(); // ahora SIN sonido
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>Attack: rota y dispara. Si sale de rango/visión -> Alert.</summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            ShowVisuals();
            UpdateRotation();

            if (playerDistance <= attackRange && shooter.CanShoot())
            {
                shooter.Shoot();
                PlayShootSfx(); // <-- único SFX

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
            StartHide();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    /// <summary>Muerte: colapso y destroy.</summary>
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
    void ShowVisuals()
    {
        if (visualChild != null && !visualChild.gameObject.activeSelf)
            visualChild.gameObject.SetActive(true);
    }

    void HideVisuals()
    {
        if (visualChild != null && visualChild.gameObject.activeSelf)
            visualChild.gameObject.SetActive(false);
    }

    // Oculta con animación (sin SFX)
    void StartHide()
    {
        if (procAnim != null)
            procAnim.PlayHide(() => { HideVisuals(); });
        else
            HideVisuals();
    }
    #endregion

    #region Audio Helpers (solo disparo)
    void PlayShootSfx()
    {
        if (!shootSfx || !audioSource) return;
        audioSource.pitch = Random.Range(shootPitchRandom.x, shootPitchRandom.y);
        audioSource.PlayOneShot(shootSfx, shootVolume);

        // Alternativa sin AudioSource (sin pitch):
        // AudioSource.PlayClipAtPoint(shootSfx, transform.position, shootVolume);
    }
    #endregion

    // (Opcional) Forzar “nudge” manual si te sirve:
    public void NudgeAimLeft() => procAnim?.PlayAimNudge(-1f);
    public void NudgeAimRight() => procAnim?.PlayAimNudge(+1f);
}
