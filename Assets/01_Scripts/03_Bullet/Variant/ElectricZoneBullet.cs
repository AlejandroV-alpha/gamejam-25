using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bala electrica que crea una zona de daño y aturdimiento
/// al alcanzar cierta distancia o al impactar directamente.
/// </summary>
public class ElectricZoneBullet : BaseBullet
{
    #region Inspector Variables
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    [Header("Zone Settings")]
    [SerializeField] float radius = 1f;
    [SerializeField] float maxDistance = 2f;

    [Header("Stun Settings")]
    [SerializeField] float stunDuration = 1f;
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
        if (!zoneActive && Vector2.Distance(transform.position, startPosition) >= maxDistance)
        {
            zoneActive = true;

            // Stop movement
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        if (zoneActive)
        {
            ApplyElectricZone();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endregion

    #region Override Methods
    /// <summary>
    /// Maneja el impacto directo antes de alcanzar la distancia maxima.
    /// </summary>
    /// <param name="collision">Colision detectada por el sistema de fisicas.</param>
    protected override void HandleImpact(Collider2D collision)
    {
        ApplyEffect(collision);
    }
    #endregion

    #region Electric Zone Logic
    /// <summary>
    /// Aplica los efectos de daño y aturdimiento dentro del radio de la zona electrica.
    /// </summary>
    void ApplyElectricZone()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, validImpactLayers);

        HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

        foreach (var t in targets)
        {
            if (alreadyHit.Contains(t))
            {
                continue;
            }

            ApplyEffect(t);
            alreadyHit.Add(t);
        }
    }

    /// <summary>
    /// Aplica los efectos de daño y aturdimiento a un objetivo especifico.
    /// </summary>
    /// <param name="target">Colision detectada por el sistema de fisicas.</param>
    void ApplyEffect(Collider2D target)
    {
        if (target.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        if (target.TryGetComponent(out IStunnable stunnable))
        {
            stunnable.Stun(stunDuration);
        }

        // Generic visual effect
        SpawnImpactEffect();

        // Destroy the bullet
        Destroy(gameObject);
    }
    #endregion
}
