using UnityEngine;
using System.Collections.Generic;

public class ElectricZoneBullet : BaseBullet
{
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 0.5f;

    [Header("Zone Settings")]
    [SerializeField] float radius = 1f;
    [SerializeField] float maxDistance = 2f;

    [Header("Stun Settings")]
    [SerializeField] float stunDuration = 1f;

    Vector2 startPosition;
    bool zoneActive = false;

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

            // Detener movimiento
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


    protected override void HandleImpact(Collider2D collision)
    {
        // Impacto directo antes de maxDistance
        ApplyEffect(collision);
    }

    void ApplyElectricZone()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, validImpactLayers);

        HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

        foreach (var t in targets)
        {
            if (alreadyHit.Contains(t)) continue;
            ApplyEffect(t);
            alreadyHit.Add(t);
        }
    }

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

        // Efecto visual genérico
        SpawnImpactEffect();

        // Destruye la bala
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
