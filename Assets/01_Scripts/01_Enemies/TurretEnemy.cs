using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TurretEnemy : MonoBehaviour, ITakeDamage
{
    #region Variables
    [Header("Life")]
    [SerializeField] float life = 5f;

    [Header("Target detection")]
    [SerializeField] float detectionRange = 5f;   // Distancia a la que detecta al jugador
    [SerializeField] LayerMask targetLayer;       // Layer del jugador u objetivo

    [Header("Field of View")]
    [SerializeField, Range(0f, 360f)] float viewAngle = 360f; // Ángulo total de visión de la torreta
    float halfViewAngle;                                      // Calculado internamente para limitar rotación
    Quaternion initialRotation;                               // Guarda la rotación inicial de la torreta

    [Header("Properties of Rotation")]
    [SerializeField] float rotateSpeed = 120f;    // Velocidad de rotación del cañón

    [Header("Properties of Shoot")]
    [SerializeField] GameObject bulletPrefab;     // Prefab de bala
    [SerializeField] Transform firePoint;         // Punto de disparo
    [SerializeField] float fireRate = 1f;         // Tiempo entre disparos
    float fireTimer = 0f;

    [Header("Impacto/Knockback")]
    [SerializeField] float impactDecay = 8f;     // Decaimiento del knockback recibido
    Vector2 impactVelocity = Vector2.zero;

    Rigidbody2D rb;
    Transform target; // jugador detectado actualmente
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        initialRotation = transform.rotation;
        halfViewAngle = viewAngle / 2f;
    }

    void FixedUpdate()
    {
        DetectTarget();
        RotateTurret();
        HandleShooting();

        // Aplica el impacto recibido suavemente
        rb.MovePosition(rb.position + impactVelocity * Time.fixedDeltaTime);
        impactVelocity = Vector2.Lerp(impactVelocity, Vector2.zero, impactDecay * Time.fixedDeltaTime);
    }

    #region Detección y rotación
    /// <summary>
    /// Detecta al jugador u objetivo dentro del rango y ángulo de visión.
    /// </summary>
    void DetectTarget()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, targetLayer);

        if (hit != null)
        {
            Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.SignedAngle(transform.up, dirToTarget);

            if (Mathf.Abs(angleToTarget) <= halfViewAngle)
            {
                target = hit.transform;
                return;
            }
        }

        target = null;
    }

    /// <summary>
    /// Gira suavemente hacia el objetivo si está dentro del ángulo permitido,
    /// o regresa lentamente a su rotación inicial si no hay objetivo.
    /// </summary>
    void RotateTurret()
    {
        float targetAngle;

        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            float desiredAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            float relativeAngle = Mathf.DeltaAngle(initialRotation.eulerAngles.z, desiredAngle);
            relativeAngle = Mathf.Clamp(relativeAngle, -halfViewAngle, halfViewAngle);

            targetAngle = initialRotation.eulerAngles.z + relativeAngle;
        }
        else
        {
            // Sin objetivo: regresa lentamente al ángulo inicial
            targetAngle = initialRotation.eulerAngles.z;
        }

        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }
    #endregion

    #region Disparo
    /// <summary>
    /// Controla el temporizador y dispara solo si el objetivo sigue dentro del viewAngle.
    /// </summary>
    void HandleShooting()
    {
        if (target == null) return;

        // Comprobamos otra vez que el objetivo sigue dentro del ángulo antes de disparar
        Vector2 dirToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector2.SignedAngle(transform.up, dirToTarget);

        if (Mathf.Abs(angleToTarget) > halfViewAngle)
        {
            target = null; // El objetivo salió del rango visual
            return;
        }

        fireTimer += Time.fixedDeltaTime;
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    /// <summary>
    /// Instancia la bala y le asigna la dirección actual del cañón.
    /// </summary>
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDirection(firePoint.up);
        }
    }
    #endregion

    #region Impacto y vida
    /// <summary>
    /// Aplica daño a la torreta y un knockback suave.
    /// </summary>
    public void TakeDamage(float damage, Vector2 hitDirection, float knockbackForce = 0f)
    {
        life -= damage;

        if (knockbackForce > 0f)
        {
            impactVelocity += hitDirection.normalized * knockbackForce;
        }

        if (life <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Destruye la torreta.
    /// </summary>
    void Die()
    {
        Debug.Log("Torreta destruida");
        Destroy(gameObject);
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualización del campo de visión (viewAngle)
        Vector3 leftDir = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.up;
        Vector3 rightDir = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);
    }
    #endregion
}
