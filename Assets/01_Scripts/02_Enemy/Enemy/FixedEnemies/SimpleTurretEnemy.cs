using UnityEngine;

using System.Collections;

/// <summary>

/// Enemigo fijo que rota hacia el jugador y dispara cuando este entra en su rango de ataque.

/// Se pone en alerta cuando el jugador entra en un rango mayor.

/// Usa IShooter para manejar disparos y respeta obstáculos.

/// + SFX de disparo

/// </summary>

public class SimpleTurretEnemy : FixedEnemy

{

    [Header("Recoil")]

    [SerializeField] private Transform recoilTarget;

    [SerializeField] private Axis recoilAxis = Axis.Up;

    [SerializeField] private bool recoilInvert = true;

    [SerializeField] private float recoilDistance = 0.08f;

    [SerializeField] private float recoilOutTime = 0.06f;

    [SerializeField] private float recoilInTime = 0.10f;

    [SerializeField] private AnimationCurve recoilOutCurve = null;

    [SerializeField] private AnimationCurve recoilInCurve = null;

    private enum Axis { Up, Right }

    private Coroutine recoilCo;

    private Vector3 recoilBaseLocalPos;

    // ---------- AUDIO ----------

    [Header("SFX")]

    [Tooltip("Se creará uno si no existe.")]

    [SerializeField] private AudioSource audioSource;

    [Tooltip("Clip de disparo (arrastra aquí BalaHieloDron, etc.)")]

    [SerializeField] private AudioClip shootSfx;

    [Range(0f, 1f)][SerializeField] private float shootVolume = 0.9f;

    [SerializeField] private Vector2 pitchRandom = new Vector2(0.97f, 1.03f);

    [Range(0f, 1f)][SerializeField] private float spatialBlend = 1f; // 1=3D

    #region Unity Methods

    protected override void Awake()

    {

        base.Awake();

        if (!recoilTarget)

        {

            var t = transform.Find("Graphics/Turret");

            if (t) recoilTarget = t;

        }

        if (recoilOutCurve == null) recoilOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        if (recoilInCurve == null) recoilInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        if (recoilTarget) recoilBaseLocalPos = recoilTarget.localPosition;

        // AudioSource seguro

        if (!audioSource) audioSource = GetComponent<AudioSource>();

        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        audioSource.spatialBlend = spatialBlend;

        audioSource.rolloffMode = AudioRolloffMode.Linear;

        audioSource.minDistance = 2f;

        audioSource.maxDistance = 15f;

    }

    private void OnEnable()

    {

        // Si en el futuro tu shooter emite evento OnShot, suscríbete aquí.

    }

    private void OnDisable()

    {

        if (recoilTarget) recoilTarget.localPosition = recoilBaseLocalPos;

        recoilCo = null;

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

                shooter.Shoot();   // Dispara

                OnShot();          // Sonido de disparo

                PlayRecoil();      // Retroceso visual

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

    #region Audio

    private void OnShot()

    {

        if (!shootSfx || !audioSource) return;

        audioSource.pitch = UnityEngine.Random.Range(pitchRandom.x, pitchRandom.y);

        audioSource.PlayOneShot(shootSfx, shootVolume);

        // Alternativa sin AudioSource (sin pitch):

        // AudioSource.PlayClipAtPoint(shootSfx, transform.position, shootVolume);

    }

    #endregion

    #region Recoil Impl

    private void PlayRecoil()

    {

        if (!recoilTarget || !gameObject.activeInHierarchy) return;

        if (recoilCo != null) StopCoroutine(recoilCo);

        recoilCo = StartCoroutine(CoRecoil());

    }

    private IEnumerator CoRecoil()

    {

        // 1) Dirección del cañón en espacio de MUNDO

        Vector3 worldDir = (recoilAxis == Axis.Up) ? recoilTarget.up : recoilTarget.right;

        if (recoilInvert) worldDir = -worldDir;

        // 2) Convertir a ESPACIO DEL PADRE (porque vamos a mover localPosition)

        Transform parent = recoilTarget.parent;

        Vector3 parentLocalDir = parent ? parent.InverseTransformDirection(worldDir) : worldDir;

        parentLocalDir.Normalize();

        Vector3 start = recoilBaseLocalPos;

        Vector3 back = start + parentLocalDir * recoilDistance;

        // Ida

        float t = 0f;

        float outT = Mathf.Max(0.01f, recoilOutTime);

        while (t < outT)

        {

            float k = recoilOutCurve.Evaluate(t / outT);

            recoilTarget.localPosition = Vector3.LerpUnclamped(start, back, k);

            t += Time.deltaTime;

            yield return null;

        }

        recoilTarget.localPosition = back;

        // Vuelta

        t = 0f;

        float inT = Mathf.Max(0.01f, recoilInTime);

        while (t < inT)

        {

            float k = recoilInCurve.Evaluate(t / inT);

            recoilTarget.localPosition = Vector3.LerpUnclamped(back, start, k);

            t += Time.deltaTime;

            yield return null;

        }

        recoilTarget.localPosition = start;

        recoilCo = null;

    }

    #endregion

}

