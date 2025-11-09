using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara ráfagas de balas.
/// Hereda de FixedEnemy para detección y rotación. Usa IShooter para disparos.
/// Añadido: efecto de retroceso al disparar (visual, por posición local) + SFX de disparo.
/// </summary>
public class IceSentinelEnemy : FixedEnemy
{
    #region Recoil Settings
    [Header("Recoil (Retroceso)")]
    [Tooltip("Parte visual que se moverá hacia atrás al disparar (ej. FirePoint o TorretaHielo).")]
    [SerializeField] private Transform recoilPart;

    [Tooltip("Distancia del retroceso en unidades (ajusta a tu sprite/escala).")]
    [SerializeField] private float recoilDistance = 0.2f;

    [Tooltip("Velocidad del ir-y-volver del retroceso (entre 8 y 16 suele verse bien).")]
    [SerializeField] private float recoilSpeed = 12f;

    private Vector3 originalLocalPos;
    private bool isRecoiling = false;
    #endregion

    #region Audio Settings
    [Header("SFX")]
    [Tooltip("Fuente de audio para reproducir el disparo (si está vacío, se creará una).")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Clip de disparo (arrastra aquí BalaHieloDron).")]
    [SerializeField] private AudioClip shootSfx;

    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 0.9f;

    [Tooltip("Aleatorización de pitch para evitar efecto repetitivo.")]
    [SerializeField] private Vector2 pitchRandom = new Vector2(0.97f, 1.03f);

    [Tooltip("0 = 2D, 1 = 3D. Para mundo 2D con posición espacial, deja ~0.7-1.")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();

        if (recoilPart != null)
            originalLocalPos = recoilPart.localPosition;

        // Asegura AudioSource
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;   // 3D si es 1
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
    protected override void IdleBehaviour()
    {
        if (currentTarget != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    protected override void AlertBehaviour()
    {
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
            ClearRotation();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            UpdateRotation();

            if (playerDistance <= attackRange && shooter != null && shooter.CanShoot())
            {
                shooter.Shoot();
                PlayShootSfx();   // Sonido sólo cuando dispara
                ApplyRecoil();    // Retroceso visual
            }
            else
            {
                stateMachine.ChangeState(EnemyState.Alert);
            }
        }
        else
        {
            ClearRotation();
            stateMachine.ChangeState(EnemyState.Idle);
        }
    }

    protected override void DeathBehaviour()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Recoil Logic
    private void ApplyRecoil()
    {
        if (recoilPart == null || isRecoiling) return;
        StartCoroutine(RecoilCoroutine());
    }

    private IEnumerator RecoilCoroutine()
    {
        isRecoiling = true;

        Vector3 start = originalLocalPos;
        Vector3 target = originalLocalPos - Vector3.up * recoilDistance; // cambia eje si tu cañón apunta por X, etc.

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            recoilPart.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            recoilPart.localPosition = Vector3.Lerp(target, start, t);
            yield return null;
        }

        recoilPart.localPosition = start;
        isRecoiling = false;
    }
    #endregion

    #region Audio Helpers
    private void PlayShootSfx()
    {
        if (!shootSfx || !audioSource) return;

        float p = Random.Range(pitchRandom.x, pitchRandom.y);
        audioSource.pitch = p;
        audioSource.PlayOneShot(shootSfx, shootVolume);
    }
    #endregion
}
