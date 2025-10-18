using UnityEngine;

[RequireComponent(typeof(IShooter))]
[RequireComponent(typeof(RotatorTowardsTarget))]
public abstract class FixedEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Detection Settings")]
    [SerializeField] protected LayerMask playerLayer;

    [Header("Obstacle Settings")]
    [SerializeField] protected LayerMask obstacleLayer;
    #endregion

    #region Protected Fields
    /// <summary>
    /// Componente encargado de los disparos.
    /// </summary>
    protected IShooter shooter;

    /// <summary>
    /// Componente encargado de rotar hacia el objetivo.
    /// </summary>
    protected RotatorTowardsTarget rotatorTowardsTarget;

    /// <summary>
    /// Referencia al transform del jugador detectado.
    /// </summary>
    protected Transform currentTarget;

    /// <summary>
    /// Distancia actual al jugador detectado.
    /// </summary>
    protected float playerDistance;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<IShooter>();
        rotatorTowardsTarget = GetComponent<RotatorTowardsTarget>();
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
    }
    #endregion

    #region Detection and Rotation
    /// <summary>
    /// Detecta al jugador dentro del rango de alerta considerando obstaculos.
    /// </summary>
    protected virtual void UpdateDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, alertRange, playerLayer);
        if (hit != null)
        {
            Vector2 direction = (hit.transform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, hit.transform.position);

            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer | playerLayer);
            if (rayHit.collider != null && ((1 << rayHit.collider.gameObject.layer) & playerLayer) != 0)
            {
                currentTarget = hit.transform;
                playerDistance = distance;
            }
            else
            {
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
    /// Actualiza la rotacion hacia el jugador detectado.
    /// </summary>
    protected void UpdateRotation()
    {
        if (currentTarget != null)
        {
            rotatorTowardsTarget.SetTarget(currentTarget);
        }
    }

    /// <summary>
    /// Limpia la rotacion cuando no hay jugador detectado.
    /// </summary>
    protected void ClearRotation()
    {
        rotatorTowardsTarget.ClearTarget();
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos de alerta y ataque en el editor.
    /// </summary>
    protected void DrawRanges()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}
