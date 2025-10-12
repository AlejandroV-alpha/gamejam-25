using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DronEnemy : MonoBehaviour, ITakeDamage
{
    #region Variables Generales
    [Header("Life")]
    [SerializeField] float life = 5f;

    [Header("Target detection")]
    [SerializeField] float detectionRange = 5f;
    [SerializeField] LayerMask targetLayer;
    [SerializeField, Range(0f, 360f)] float viewAngle = 180f;
    float halfViewAngle;
    Quaternion initialRotation;

    [Header("Properties of Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] Vector2 patrolMin;
    [SerializeField] Vector2 patrolMax;
    [SerializeField] float desiredDistance = 3f;
    Vector2 patrolTarget;

    [Header("Properties of Shoot")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float timeBtwShoot = 0.5f;
    float shootTimer = 0f;

    [Header("Knockback")]
    [SerializeField] float impactDecay = 8f;
    Vector2 impactVelocity = Vector2.zero;

    Rigidbody2D rb;
    Transform target;

    enum DroneState { Patrol, Chase }
    DroneState currentState = DroneState.Patrol;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        initialRotation = transform.rotation;
        halfViewAngle = viewAngle / 2f;

        patrolTarget = GetRandomPatrolPoint();
    }

    void FixedUpdate()
    {
        DetectTarget();

        Vector2 moveDelta = Vector2.zero;

        if (currentState == DroneState.Patrol)
        {
            moveDelta = Patrol();
        }
        else if (currentState == DroneState.Chase)
        {
            moveDelta = Chase();
        }

        // Aplicar movimiento + knockback
        rb.MovePosition(rb.position + moveDelta * Time.fixedDeltaTime + impactVelocity * Time.fixedDeltaTime);
        impactVelocity = Vector2.Lerp(impactVelocity, Vector2.zero, impactDecay * Time.fixedDeltaTime);

        RotateDrone();

        shootTimer += Time.fixedDeltaTime;
        if (currentState == DroneState.Chase && shootTimer >= timeBtwShoot)
        {
            shootTimer = 0f;
            HandleShooting();
        }
    }

    #region Patrulla y Persecucion
    /// <summary>
    /// Calcula el movimiento de patrulla hacia el waypoint actual.
    /// </summary>
    Vector2 Patrol()
    {
        Vector2 dir = patrolTarget - rb.position;

        if (dir.magnitude < 0.1f)
        {
            patrolTarget = GetRandomPatrolPoint();
            dir = patrolTarget - rb.position;
        }

        return dir.normalized * moveSpeed;
    }

    /// <summary>
    /// Calcula el movimiento hacia el objetivo (jugador), manteniendo distancia deseada.
    /// </summary>
    Vector2 Chase()
    {
        if (target == null)
        {
            currentState = DroneState.Patrol;
            return Vector2.zero;
        }

        Vector2 dirToPlayer = (Vector2)target.position - rb.position;
        float distance = dirToPlayer.magnitude;

        Vector2 moveDir = Vector2.zero;
        if (distance > desiredDistance)
        {
            moveDir = dirToPlayer.normalized;
        }
        else if (distance < desiredDistance * 0.8f)
        {
            moveDir = -dirToPlayer.normalized;
        }

        return moveDir * moveSpeed;
    }

    /// <summary>
    /// Devuelve un punto aleatorio dentro del area de patrulla.
    /// </summary>
    Vector2 GetRandomPatrolPoint()
    {
        return new Vector2(
            Random.Range(patrolMin.x, patrolMax.x),
            Random.Range(patrolMin.y, patrolMax.y)
        );
    }
    #endregion

    #region Deteccion y rotacion
    /// <summary>
    /// Detecta si hay un objetivo dentro del rango y campo de vision.
    /// Cambia el estado de la IA a Chase o Patrol.
    /// </summary>
    void DetectTarget()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, targetLayer);

        if (hit != null)
        {
            Vector2 dirToTarget = (hit.transform.position - transform.position).normalized;
            float desiredAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg - 90f;
            float relativeAngle = Mathf.DeltaAngle(initialRotation.eulerAngles.z, desiredAngle);

            if (Mathf.Abs(relativeAngle) <= halfViewAngle)
            {
                target = hit.transform;
                currentState = DroneState.Chase;
                return;
            }
        }

        target = null;
        currentState = DroneState.Patrol;
    }

    /// <summary>
    /// Rota el dron hacia su objetivo actual (jugador o punto de patrulla) de manera suave.
    /// </summary>
    void RotateDrone()
    {
        float desiredAngle;

        if (currentState == DroneState.Chase && target != null)
        {
            Vector2 dirToTarget = ((Vector2)target.position - rb.position).normalized;
            desiredAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg - 90f;
        }
        else
        {
            Vector2 dirToPatrol = (patrolTarget - rb.position).normalized;
            desiredAngle = Mathf.Atan2(dirToPatrol.y, dirToPatrol.x) * Mathf.Rad2Deg - 90f;
        }

        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, desiredAngle, 360f * Time.fixedDeltaTime));
    }
    #endregion

    #region Disparo
    /// <summary>
    /// Maneja el disparo del dron hacia el objetivo si este esta dentro del campo de vision.
    /// </summary>
    void HandleShooting()
    {
        if (target == null || bulletPrefab == null || firePoint == null) return;

        Vector2 dirToTarget = ((Vector2)target.position - rb.position).normalized;
        float desiredAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg - 90f;
        float relativeAngle = Mathf.DeltaAngle(initialRotation.eulerAngles.z, desiredAngle);

        if (Mathf.Abs(relativeAngle) <= halfViewAngle)
        {
            Shoot();
        }
    }

    /// <summary>
    /// Instancia la bala y la direcciona hacia el objetivo.
    /// </summary>
    void Shoot()
    {
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
    /// Aplica daño al dron, y opcionalmente knockback.
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
    /// Destruye el dron y registra la muerte en consola.
    /// </summary>
    void Die()
    {
        Destroy(gameObject);
        Debug.Log("Drone destruido");
    }
    #endregion

    #region Gizmos
    /// <summary>
    /// Dibuja en el editor el rango de deteccion, campo de vision y zona de patrulla.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftDir = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.up;
        Vector3 rightDir = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(patrolMin.x, patrolMin.y), new Vector3(patrolMax.x, patrolMin.y));
        Gizmos.DrawLine(new Vector3(patrolMax.x, patrolMin.y), new Vector3(patrolMax.x, patrolMax.y));
        Gizmos.DrawLine(new Vector3(patrolMax.x, patrolMax.y), new Vector3(patrolMin.x, patrolMax.y));
        Gizmos.DrawLine(new Vector3(patrolMin.x, patrolMax.y), new Vector3(patrolMin.x, patrolMin.y));
    }
    #endregion
}
