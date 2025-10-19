using UnityEngine;

/// <summary>
/// Bala teledirigida tipo cohete que sigue al jugador.
/// </summary>
public class RocketBullet : BaseBullet
{
    [Header("Rocket Settings")]
    [SerializeField] private float rotationSpeed = 300f; // grados por segundo
    [SerializeField] private Transform target;

    protected override void Awake()
    {
        base.Awake();

        // Buscar automáticamente al jugador si no se asigna target
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Dirección hacia el objetivo
            Vector2 dir = (target.position - transform.position).normalized;

            // Actualizar la dirección para TakeDamage
            direction = dir;

            // Rotación suave hacia el target
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float newRotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);

            // Movimiento hacia el target
            rb.linearVelocity = dir * moveSpeed;
        }
    }

    #region Collision Behavior
    /// <summary>
    /// Logica personalizada al impactar: destruye cohete al impactar con cualquier objeto válido
    /// y llama a efectos de impacto.
    /// </summary>
    /// <param name="collision">Collider con el que impactó</param>
    protected override void HandleImpact(Collider2D collision)
    {
        Destroy(gameObject);
    }
    #endregion
}
