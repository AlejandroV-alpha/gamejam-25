using UnityEngine;
using System.Collections; // para la corrutina de recoil

/// <summary>
/// Torreta de plasma fija que dispara 3 balas en abanico (2D).
/// Se sobrecalienta tras un numero de rafagas y se enfria despues de un tiempo.
/// Hereda de FixedEnemy para deteccion y rotacion.
/// </summary>
public class PlasmaTurretEnemy : FixedEnemy
{
    // -------- Aiming (2D) --------
    [Header("Aiming 2D")]
    [SerializeField] private float turnSpeed = 540f;        // grados/seg para suavizar el giro
    [SerializeField] private float barrelVisualOffset = -90f; // corrige la PUNTA del sprite (0, -90, 90, 180)

    // -------- Recoil (2D) --------
    [Header("Recoil 2D")]
    [SerializeField] private Transform recoilPart;          // pieza que se empuja (puede ser este mismo transform)
    [SerializeField] private bool barrelPointsUp = true;    // true: el cañon apunta por Up; false: por Right
    [SerializeField] private float recoilDistance = 0.15f;
    [SerializeField] private float recoilOutTime = 0.05f;
    [SerializeField] private float recoilReturnTime = 0.08f;
    [SerializeField] private AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _recoilInitLocalPos;
    private bool _recoiling;

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();

        if (!recoilPart) recoilPart = transform;
        _recoilInitLocalPos = recoilPart.localPosition;

        // Forzar Z=0 al inicio (2D)
        var p = transform.position;
        transform.position = new Vector3(p.x, p.y, 0f);
    }

    protected override void Update()
    {
        base.Update();

        // Mantener Z=0 (2D)
        var p = transform.position;
        if (Mathf.Abs(p.z) > 0.0001f)
            transform.position = new Vector3(p.x, p.y, 0f);
    }

    void OnDrawGizmosSelected()
    {
        DrawRanges();
        // Gizmo: eje de disparo (Up o Right según config)
        Gizmos.color = Color.red;
        Vector3 dir = (barrelPointsUp ? transform.up : transform.right) * 0.8f;
        Gizmos.DrawLine(transform.position, transform.position + dir);
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
            UpdateRotation();          // si tu clase base ya hace algo útil
            AimSelfToTarget2D();       // apunta con offset visual

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
            AimSelfToTarget2D();

            if (playerDistance <= attackRange && shooter != null && shooter.CanShoot())
            {
                shooter.Shoot();   // Asegura que la bala use transform.up o transform.right (ver nota)
                StartRecoil2D();
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

    // ===================== Helpers 2D =====================
    // Alinea la PUNTA del sprite hacia el target usando un offset visual.
    private void AimSelfToTarget2D()
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - transform.position;
        dir.z = 0f; // 2D: plano XY
        if (dir.sqrMagnitude < 0.0001f) return;

        // Ángulo hacia el jugador (derecha = 0°)
        float rawTarget = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Compensación para que la PUNTA del cañón apunte bien
        float desiredZ = rawTarget + barrelVisualOffset;

        float currentZ = transform.eulerAngles.z;
        float nextZ = Mathf.MoveTowardsAngle(currentZ, desiredZ, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, nextZ);
    }

    private void StartRecoil2D()
    {
        if (!_recoiling && recoilPart != null)
            StartCoroutine(Co_Recoil2D());
    }

    private IEnumerator Co_Recoil2D()
    {
        _recoiling = true;

        // Retroceso contrario al eje de disparo
        Vector3 localRecoilDir = barrelPointsUp ? Vector3.down : Vector3.left;

        // Ida
        float t = 0f;
        while (t < recoilOutTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / recoilOutTime);
            float eased = recoilCurve.Evaluate(k);
            recoilPart.localPosition = _recoilInitLocalPos + localRecoilDir * (recoilDistance * eased);
            yield return null;
        }

        // Vuelta
        t = 0f;
        while (t < recoilReturnTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / recoilReturnTime);
            float eased = recoilCurve.Evaluate(1f - k);
            recoilPart.localPosition = _recoilInitLocalPos + localRecoilDir * (recoilDistance * eased);
            yield return null;
        }

        recoilPart.localPosition = _recoilInitLocalPos;
        _recoiling = false;
    }
}
