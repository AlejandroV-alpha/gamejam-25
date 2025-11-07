using UnityEngine;

[DisallowMultipleComponent]
public class BulletImpactFX2D : MonoBehaviour
{
    [Header("FX de impacto (Prefab con Particle System)")]
    [SerializeField] private GameObject impactFxPrefab;

    [Header("Capas que deben generar impacto")]
    [SerializeField] private LayerMask impactLayers;

    [Header("Partículas de vuelo (opcionales)")]
    [SerializeField] private ParticleSystem moveParticles; // hijo de la bala
    [SerializeField] private TrailRenderer trail;          // si usas trail

    [Header("Destruir o Pool")]
    [SerializeField] private bool usePooling = false;

    // --- TRIGGER ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsInMask(other.gameObject.layer)) return;
        Vector2 hitPos = other.ClosestPoint(transform.position);
        Explode(hitPos);
    }

    // --- COLISIÓN FÍSICA ---
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsInMask(col.collider.gameObject.layer)) return;
        Vector2 hitPos = col.contactCount > 0 ? col.GetContact(0).point : (Vector2)transform.position;
        Explode(hitPos);
    }

    private void Explode(Vector2 pos)
    {
        // 1) Instanciar partículas de impacto
        if (impactFxPrefab != null)
            Instantiate(impactFxPrefab, pos, Quaternion.identity);

        // 2) Apagar partículas de vuelo para que no “corten” feo
        if (moveParticles != null)
            moveParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (trail != null)
            trail.emitting = false;

        // 3) Destruir bala (o devolver a pool)
        if (usePooling)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }

    private bool IsInMask(int layer) => (impactLayers.value & (1 << layer)) != 0;
}
