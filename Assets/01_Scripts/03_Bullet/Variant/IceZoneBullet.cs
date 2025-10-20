using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bala de hielo que, al alcanzar su distancia maxima, crea una zona congelante.
/// Los objetivos dentro del radio son ralentizados mientras dure el efecto.
/// </summary>
public class IceZoneBullet : BaseBullet
{
    #region Inspector Variables
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    [Header("Zone Settings")]
    [SerializeField] float radius = 1.5f;
    [SerializeField] float maxDistance = 3f;

    [Header("Slow Settings")]
    [SerializeField] float slowDuration = 2f;
    [SerializeField, Tooltip("Multiplicador de velocidad (0.6 = 40% mas lento)")]
    float speedMultiplier = 0.6f;
    #endregion

    #region Private Fields
    Vector2 startPosition;
    bool zoneActive = false;
    #endregion

    #region Unity Methods
    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Activa la zona cuando alcanza la distancia maxima
        if (!zoneActive && Vector2.Distance(transform.position, startPosition) >= maxDistance)
        {
            zoneActive = true;

            // Detiene el movimiento de la bala
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // Mientras la zona este activa, aplica el efecto de congelacion
        if (zoneActive)
        {
            ApplyFreezeZone();
        }
    }
    #endregion

    #region Core Logic
    /// <summary>
    /// Aplica el efecto directo si impacta antes de llegar a maxDistance.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        ApplyFreezeEffect(collision);
    }

    /// <summary>
    /// Detecta objetivos dentro del radio y les aplica la ralentizacion.
    /// </summary>
    void ApplyFreezeZone()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, validImpactLayers);

        HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

        foreach (var t in targets)
        {
            if (alreadyHit.Contains(t))
            {
                continue;
            }

            ApplyFreezeEffect(t);
            alreadyHit.Add(t);
        }
    }

    /// <summary>
    /// Aplica el efecto de ralentizacion al objetivo afectado.
    /// </summary>
    /// <param name="target">Colision detectada por el sistema de fisicas.</param>
    void ApplyFreezeEffect(Collider2D target)
    {
        if (target.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        if (target.TryGetComponent(out ISlowable slowable))
        {
            slowable.Slow(slowDuration, speedMultiplier);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // No se destruye al aplicar efecto (la zona persiste hasta que se destruya por tiempo)
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endregion
}
