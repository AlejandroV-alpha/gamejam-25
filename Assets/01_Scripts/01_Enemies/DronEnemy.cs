using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DronEnemy : MonoBehaviour, ITakeDamage
{
    #region Variables Generales
    [Header("Vida")]
    [SerializeField] private float life = 5f;

    [Header("Detección del jugador")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 180f;
    private float halfViewAngle;
    private Quaternion initialRotation;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector2 patrolMin;
    [SerializeField] private Vector2 patrolMax;
    [SerializeField] private float desiredDistance = 3f;
    private Vector2 patrolTarget;

    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float timeBtwShoot = 0.5f;
    private float shootTimer = 0f;

    [Header("Impacto / Knockback")]
    [SerializeField] private float impactDecay = 8f;
    private Vector2 impactVelocity = Vector2.zero;

    private Rigidbody2D rb;
    private Transform target;

    private enum DroneState { Patrol, Chase }
    private DroneState currentState = DroneState.Patrol;
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

    #region Patrulla y Persecución
    private Vector2 Patrol()
    {
        Vector2 dir = patrolTarget - rb.position;

        if (dir.magnitude < 0.1f)
        {
            patrolTarget = GetRandomPatrolPoint();
            dir = patrolTarget - rb.position;
        }

        return dir.normalized * moveSpeed;
    }

    private Vector2 Chase()
    {
        if (target == null)
        {
            currentState = DroneState.Patrol;
            return Vector2.zero;
        }

        Vector2 dirToPlayer = (Vector2)target.position - rb.position;
        float distance = dirToPlayer.magnitude;

        // Mantener distancia deseada
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

    private Vector2 GetRandomPatrolPoint()
    {
        return new Vector2(
            Random.Range(patrolMin.x, patrolMax.x),
            Random.Range(patrolMin.y, patrolMax.y)
        );
    }
    #endregion

    #region Detección y rotación
    private void DetectTarget()
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

    private void RotateDrone()
    {
        float desiredAngle;

        if (currentState == DroneState.Chase && target != null)
        {
            // Apunta hacia el jugador
            Vector2 dirToTarget = ((Vector2)target.position - rb.position).normalized;
            desiredAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg - 90f;
        }
        else
        {
            // Apunta hacia su punto de patrulla
            Vector2 dirToPatrol = (patrolTarget - rb.position).normalized;
            desiredAngle = Mathf.Atan2(dirToPatrol.y, dirToPatrol.x) * Mathf.Rad2Deg - 90f;
        }

        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, desiredAngle, 360f * Time.fixedDeltaTime));
    }
    #endregion

    #region Disparo
    private void HandleShooting()
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

    private void Shoot()
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

    private void Die()
    {
        Destroy(gameObject);
        Debug.Log("Drone destruido");
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Campo de visión
        Vector3 leftDir = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.up;
        Vector3 rightDir = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);

        // Zona de patrulla
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(patrolMin.x, patrolMin.y), new Vector3(patrolMax.x, patrolMin.y));
        Gizmos.DrawLine(new Vector3(patrolMax.x, patrolMin.y), new Vector3(patrolMax.x, patrolMax.y));
        Gizmos.DrawLine(new Vector3(patrolMax.x, patrolMax.y), new Vector3(patrolMin.x, patrolMax.y));
        Gizmos.DrawLine(new Vector3(patrolMin.x, patrolMax.y), new Vector3(patrolMin.x, patrolMin.y));
    }
    #endregion
}
