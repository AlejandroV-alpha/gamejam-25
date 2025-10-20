using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseBullet : MonoBehaviour
{
    #region Inspector Variables
    [Header("Movement and Lifetime")]
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected float timeToDestroy = 5f;

    [Header("Collision Settings")]
    [Tooltip("Layers that this bullet can interact with")]
    [SerializeField] protected LayerMask validImpactLayers;

    [Header("Energy Settings")]
    [SerializeField] protected float energyCost = 0f;
    #endregion

    #region Protected Fields
    protected Rigidbody2D rb;
    protected Vector2 direction;
    #endregion

    #region Unity Methods
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    protected virtual void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Filtra capas no válidas
        if (((1 << collision.gameObject.layer) & validImpactLayers) == 0)
        {
            return;
        }

        // Efecto visual genérico
        SpawnImpactEffect();

        // Lógica específica de la subclase
        HandleImpact(collision);
    }
    #endregion

    #region Launch
    public virtual void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
    #endregion

    #region Abstract and Virtual Methods
    /// <summary>
    /// Lógica particular de la bala al impactar (daño, energía, etc.)
    /// </summary>
    protected abstract void HandleImpact(Collider2D collision);

    protected virtual void SpawnImpactEffect()
    {
        
    }
    #endregion

    #region utilities
    public virtual float GetEnergyCost()
    {
        return energyCost;
    }
    #endregion
}
