using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara cuando este entra en su rango de ataque.
/// Se pone en alerta cuando el jugador entra en un rango mayor.
/// Usa IShooter para manejar disparos y respeta obstaculos.
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
    }

    protected override void Update()
    {
        base.Update();
    }

    void OnDisable()
    {
        if (recoilTarget) recoilTarget.localPosition = recoilBaseLocalPos;
        recoilCo = null;
    }

    void OnDrawGizmosSelected()
    {
        DrawRanges();
    }
    #endregion

    #region Behaviour Overrides
    /// <summary>
    /// Comportamiento en estado Idle: detecta jugador y cambia a alerta si es necesario.
    /// </summary>
    protected override void IdleBehaviour()
    {
        if (currentTarget != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Comportamiento en estado Alert: apunta al jugador y cambia a ataque si esta en rango.
    /// </summary>
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

    /// <summary>
    /// Comportamiento en estado Attack: dispara al jugador si esta en rango.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            UpdateRotation();
            if (playerDistance <= attackRange && shooter != null && shooter.CanShoot())
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
            ClearRotation();
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

    #region Recoil Impl
    private void PlayRecoil()
    {
        if (!recoilTarget || !gameObject.activeInHierarchy) return;
        if (recoilCo != null) StopCoroutine(recoilCo);
        recoilCo = StartCoroutine(CoRecoil());
    }

    private IEnumerator CoRecoil()
    {
        // Dirección del cañón según configuración (Up o Right)
        Vector3 worldDir = (recoilAxis == Axis.Up) ? recoilTarget.up : recoilTarget.right;
        if (recoilInvert) worldDir = -worldDir;

        // Convertir a espacio local
        Vector3 localDir = recoilTarget.InverseTransformDirection(worldDir);

        Vector3 start = recoilBaseLocalPos;
        Vector3 back = start + localDir * recoilDistance;

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
