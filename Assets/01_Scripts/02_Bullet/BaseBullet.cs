using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseBullet : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected float timeToDestroy = 5f;
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected float knockbackForce = 0f;
    [Tooltip("Objects that can be damaged")]
    [SerializeField] protected LayerMask validImpactLayers;

    protected Rigidbody2D rb;
    protected Vector2 direction;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    protected virtual void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    #region Launch Logic
    /// <summary>
    /// Lanza la bala en la direccion indicada.
    /// </summary>
    /// <param name="dir">Direccion normalizada del disparo</param>
    public virtual void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
    #endregion

    #region Collision Logic
    /// <summary>
    /// Gestiona la colision de la bala con otros objetos.
    /// Aplica daño comun y luego llama al comportamiento especial de cada bala.
    /// </summary>
    /// <param name="collision">Collider con el que impacta la bala</param>
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar capas no permitidas
        if (((1 << collision.gameObject.layer) & validImpactLayers) == 0)
        {
            return;
        }

        // Aplicar daño comun si implementa ITakeDamage
        if (collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damage, direction, knockbackForce);
        }

        // Llamar a la logica especial definida por cada bala
        OnHit(collision);
    }

    /// <summary>
    /// Evento principal al impactar. Llama a efectos comunes y a la logica personalizada.
    /// </summary>
    /// <param name="collision">Collider con el que impacto la bala</param>
    protected virtual void OnHit(Collider2D collision)
    {
        // Efectos comunes: particulas, sonido, cam shake, etc
        SpawnImpactEffect();

        // Lógica específica de cada tipo de bala
        HandleImpact(collision);
    }

    /// <summary>
    /// Logica personalizada al impactar, debe implementarse en cada bala concreta.
    /// </summary>
    /// <param name="collision">Collider con el que impacto la bala</param>
    protected abstract void HandleImpact(Collider2D collision);
    #endregion

    #region Utilities
    /// <summary>
    /// Permite instanciar efectos de impacto (particulas, sonido, etc)
    /// </summary>
    protected virtual void SpawnImpactEffect()
    {
        // Por defecto no hace nada. Las balas hijas pueden sobreescribir.
    }
    #endregion
}
