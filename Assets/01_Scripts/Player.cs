using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour, ITakeDamage
{
    #region Variables Generales
    [Header("Life")]
    [SerializeField] float life = 10f;

    [Header("Properties of Movement")]
    [SerializeField] float moveSpeedMax = 2.5f;
    [SerializeField] float moveSpeedMaxBackward = 1.5f;
    [SerializeField] float moveAcceleration = 5f;
    [SerializeField] float moveDeceleration = 5f;

    [Header("Properties of Rotation")]
    [SerializeField] float rotateSpeedMax = 130f;
    [SerializeField] float rotateAcceleration = 400f;
    [SerializeField] float rotateDeceleration = 400f;

    [Header("Properties of Shoot")]
    [SerializeField] KeyCode shootKey = KeyCode.Space;
    [SerializeField] KeyCode changeBulletKey = KeyCode.E;
    [SerializeField] BulletMode currentBullet = BulletMode.Damage;
    [SerializeField] GameObject bulletDamagePrefab;
    [SerializeField] GameObject bulletEnergyPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float timeBtwShoot = 0.5f;

    [Header("Impact Feedback")]
    [SerializeField] float impactDecay = 8f;

    float moveSpeed = 0f;
    float rotateSpeed = 0f;
    float timer = 0f;
    bool canShoot = true;

    Vector2 impactVelocity = Vector2.zero;

    Rigidbody2D rb;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
    }

    void Update()
    {
        HandleShootTimer();
        HandleShootInput();
        HandleChangeBulletInput();
    }

    void FixedUpdate()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Movimiento + impacto suave
        Vector2 moveDir = transform.up * GetTargetSpeed(verticalInput);
        rb.MovePosition(rb.position + (moveDir + impactVelocity) * Time.fixedDeltaTime);

        HandleRotation(horizontalInput);

        // Decaimiento del impacto
        impactVelocity = Vector2.Lerp(impactVelocity, Vector2.zero, impactDecay * Time.fixedDeltaTime);
    }

    #region Movimiento Player
    /// <summary>
    /// Calcula la velocidad de movimiento del jugador dependiendo del input vertical.
    /// Aplica aceleración o desaceleración suave usando MoveTowards.
    /// </summary>
    /// <param name="verticalInput">Valor de entrada vertical (-1 a 1)</param>
    /// <returns>Velocidad actual a aplicar</returns>
    float GetTargetSpeed(float verticalInput)
    {
        float targetSpeed = 0f;
        if (verticalInput > 0) targetSpeed = moveSpeedMax;
        else if (verticalInput < 0) targetSpeed = -moveSpeedMaxBackward;

        float accel = (verticalInput != 0f) ? moveAcceleration : moveDeceleration;
        moveSpeed = Mathf.MoveTowards(moveSpeed, targetSpeed, accel * Time.fixedDeltaTime);
        return moveSpeed;
    }

    /// <summary>
    /// Maneja la rotación del jugador según el input horizontal.
    /// Aplica aceleración y desaceleración suave usando MoveTowards.
    /// </summary>
    /// <param name="horizontalInput">Valor de entrada horizontal (-1 a 1)</param>
    void HandleRotation(float horizontalInput)
    {
        float targetRotateSpeed = rotateSpeedMax * Mathf.Abs(horizontalInput);
        float accel = (horizontalInput != 0f) ? rotateAcceleration : rotateDeceleration;
        rotateSpeed = Mathf.MoveTowards(rotateSpeed, targetRotateSpeed, accel * Time.fixedDeltaTime);

        float rotationAmount = -rotateSpeed * Mathf.Sign(horizontalInput) * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }
    #endregion

    #region Disparo Player
    /// <summary>
    /// Controla el temporizador entre disparos para limitar la frecuencia de disparo.
    /// </summary>
    void HandleShootTimer()
    {
        if (!canShoot)
        {
            timer += Time.deltaTime;
            if (timer >= timeBtwShoot)
            {
                timer = 0f;
                canShoot = true;
            }
        }
    }

    /// <summary>
    /// Detecta la entrada de disparo y genera la bala correspondiente.
    /// </summary>
    void HandleShootInput()
    {
        if (canShoot && Input.GetKeyDown(shootKey))
        {
            GameObject prefabToUse = (currentBullet == BulletMode.Damage) ? bulletDamagePrefab : bulletEnergyPrefab;
            GameObject bulletObj = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.SetDirection(firePoint.up);

            canShoot = false;
        }
    }

    /// <summary>
    /// Cambia el tipo de bala activa al presionar la tecla definida.
    /// </summary>
    void HandleChangeBulletInput()
    {
        if (Input.GetKeyDown(changeBulletKey))
        {
            currentBullet = (currentBullet == BulletMode.Damage) ? BulletMode.Energy : BulletMode.Damage;
        }
    }
    #endregion

    #region Impacto Player
    /// <summary>
    /// Aplica daño y un knockback al jugador.
    /// Knockback es suave y temporal para dar sensación de impacto.
    /// </summary>
    /// <param name="damage">Cantidad de vida a restar</param>
    /// <param name="hitDirection">Dirección desde la que recibió el impacto</param>
    /// <param name="knockbackForce">Magnitud de la fuerza de retroceso</param>
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
    /// Maneja la muerte del jugador.
    /// </summary>
    void Die()
    {
        Debug.Log("Player muerto");
        Destroy(gameObject);
    }
    #endregion
}

public enum BulletMode
{
    Damage,
    Energy
}
