using UnityEngine;

/// <summary>
/// Clase base para enemigos voladores que heredan de BaseEnemy.  
/// Implementa lógica común de movimiento, patrullaje, rotación y detección abstracta.  
/// Permite definir comportamientos específicos de detección (jugadores o aliados) en las subclases.  
/// Soporta la asignación de capas ignoradas para evitar colisiones no deseadas.
/// </summary>
[RequireComponent(typeof(RotatorTowardsTarget))]
[RequireComponent(typeof(IShooter))]
public abstract class FlyingEnemy : BaseEnemy
{
    #region Inspector Variables
    [Header("Patrol Settings")]
    [SerializeField] protected float patrolRadius = 5f;
    [SerializeField] protected float patrolSpeed = 2f;
    [SerializeField] protected float maxDistanceFromOrigin = 10f;

    [Header("Follow Settings")]
    [SerializeField] protected float followSpeed = 3f;

    [Header("Collision Layers to Ignore")]
    [SerializeField] protected LayerMask ignoreCollisionLayers;
    #endregion

    #region Protected Fields
    protected IShooter shooter;
    protected RotatorTowardsTarget rotator;
    protected Transform currentTarget;
    protected Vector2 patrolOrigin;
    protected Vector2 patrolTarget;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        shooter = GetComponent<IShooter>();
        rotator = GetComponent<RotatorTowardsTarget>();
        patrolOrigin = transform.position;
        SetRandomPatrolTarget();

        int layer = gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            if (((ignoreCollisionLayers.value >> i) & 1) == 1)
            {
                Physics2D.IgnoreLayerCollision(layer, i, true);
            }
        }
    }

    protected override void Update()
    {
        UpdateDetection();
        base.Update();
    }
    #endregion

    #region Abstract Detection
    /// <summary>
    /// Metodo abstracto que debe implementar la deteccion del jugador o aliados.
    /// </summary>
    protected abstract void UpdateDetection();
    #endregion

    #region Movement Logic
    /// <summary>
    /// Mueve al enemigo hacia una posicion objetivo con una velocidad dada.
    /// </summary>
    protected void MoveToTarget(Vector2 targetPos, float speed)
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        Vector2 nextPos = (Vector2)transform.position + direction * speed * Time.deltaTime;

        if (Vector2.Distance(patrolOrigin, nextPos) <= maxDistanceFromOrigin)
        {
            transform.position = nextPos;
        }
        else
        {
            Vector2 dirFromOrigin = (nextPos - patrolOrigin).normalized;
            transform.position = patrolOrigin + dirFromOrigin * maxDistanceFromOrigin;
        }
    }

    /// <summary>
    /// Define un nuevo punto aleatorio de patrullaje dentro del radio establecido.
    /// </summary>
    protected void SetRandomPatrolTarget()
    {
        patrolTarget = patrolOrigin + Random.insideUnitCircle * patrolRadius;
    }
    #endregion

    #region Debug Gizmos
    /// <summary>
    /// Dibuja los rangos de patrulla y puntos de referencia en la escena.
    /// </summary>
    protected void DrawFlyingRanges()
    {
        DrawRanges();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(patrolOrigin, patrolRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(patrolTarget, 0.2f);
    }
    #endregion
}
