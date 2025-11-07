using UnityEngine;

[DisallowMultipleComponent]
public class BulletFX : MonoBehaviour
{
    [Header("Move FX (child)")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private TrailRenderer trail; // opcional

    [Header("Impact FX")]
    [SerializeField] private GameObject impactFxPrefab;
    [SerializeField] private LayerMask impactLayers; // capas que deben generar impacto

    // Si tu bala usa Collider2D con Trigger, usa OnTriggerEnter2D;
    // si no, OnCollisionEnter2D. Puedes dejar ambos.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, impactLayers))
        {
            SpawnImpact(other.ClosestPoint(transform.position));
            Destroy(gameObject); // o devuelve al pool si usas pooling
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.contactCount > 0 && IsInLayerMask(col.collider.gameObject.layer, impactLayers))
        {
            Vector2 hitPos = col.GetContact(0).point;
            SpawnImpact(hitPos);
            Destroy(gameObject);
        }
    }

    private void SpawnImpact(Vector2 position)
    {
        if (impactFxPrefab != null)
        {
            Instantiate(impactFxPrefab, position, Quaternion.identity);
        }

        // Apagar emisión suave para que el trail/particles no “corten” feo
        if (moveParticles != null)
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (trail != null)
            trail.emitting = false;
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
