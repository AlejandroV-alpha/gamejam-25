using UnityEngine;

public class RocketBullet : BaseBullet
{
    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 10f;
    [SerializeField] float knockbackForce = 5f;

    [Header("Rocket Settings")]
    [SerializeField] float rotationSpeed = 300f;
    [SerializeField] Transform target;

    protected override void Awake()
    {
        base.Awake();
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        direction = dir;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float newRotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);

        rb.linearVelocity = dir * moveSpeed;
    }

    protected override void HandleImpact(Collider2D collision)
    {
        if (collision.TryGetComponent(out ITakeDamage damageable))
        {
            damageable.TakeDamage(damageAmount, direction, knockbackForce);
        }

        Destroy(gameObject);
    }
}
