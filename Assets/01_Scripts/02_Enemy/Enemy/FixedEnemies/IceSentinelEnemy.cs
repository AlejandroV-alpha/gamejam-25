using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo fijo que rota hacia el jugador y dispara ráfagas de balas.
/// Hereda de FixedEnemy para detección y rotación. Usa IShooter para disparos.
/// Añadido: efecto de retroceso al disparar (visual, por posición local).
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

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();

        if (recoilPart != null)
            originalLocalPos = recoilPart.localPosition;
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
    /// Comportamiento en estado Idle: sin jugador detectado, mantiene rotación inicial.
    /// </summary>
    protected override void IdleBehaviour()
    {
        if (currentTarget != null)
        {
            stateMachine.ChangeState(EnemyState.Alert);
        }
    }

    /// <summary>
    /// Comportamiento en estado Alert: rota hacia el jugador y cambia a ataque si está en rango.
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
    /// Comportamiento en estado Attack: dispara ráfagas si el jugador está en rango.
    /// </summary>
    protected override void AttackBehaviour()
    {
        if (currentTarget != null)
        {
            UpdateRotation();

            if (playerDistance <= attackRange && shooter != null && shooter.CanShoot())
            {
                shooter.Shoot();
                ApplyRecoil(); // ? activa el retroceso cada disparo
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

    #region Recoil Logic
    /// <summary>
    /// Lanza la corrutina de retroceso si no está ya en curso.
    /// </summary>
    private void ApplyRecoil()
    {
        if (recoilPart == null || isRecoiling) return;
        StartCoroutine(RecoilCoroutine());
    }

    /// <summary>
    /// Mueve la pieza hacia atrás y la regresa suavemente a su posición original.
    /// Nota: usa el eje local Y como dirección de “salida del cañón”.
    /// Si tu cañón usa otro eje, cambia Vector3.up por el que corresponda (Vector3.right, etc.).
    /// </summary>
    private IEnumerator RecoilCoroutine()
    {
        isRecoiling = true;

        // Posiciones en espacio local
        Vector3 start = originalLocalPos;
        Vector3 target = originalLocalPos - Vector3.up * recoilDistance; // cambia el eje si tu cañón apunta por X, etc.

        // Ir (hacia atrás)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            recoilPart.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        // Volver (hacia adelante)
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
}
