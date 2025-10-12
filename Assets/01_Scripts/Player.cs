using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    #region Variables
    [Header("Properties of Movement")]
    [SerializeField] float moveSpeedMax = 2.5f;         // Velocidad maxima hacia adelante
    [SerializeField] float moveSpeedMaxBackward = 1.5f; // Velocidad maxima hacia atras
    [SerializeField] float moveAcceleration = 5f;       // Velocidad de aceleracion
    [SerializeField] float moveDeceleration = 5f;       // Velocidad de desaceleracion

    [Header("Properties of Rotation")]
    [SerializeField] float rotateSpeedMax = 130f;       // Velocidad maxima de rotacion
    [SerializeField] float rotateAcceleration = 400f;   // Aceleracion de rotacion
    [SerializeField] float rotateDeceleration = 400f;   // Desaceleracion de rotacion

    [Header("Properties of Shoot")]
    [SerializeField] KeyCode shootKey = KeyCode.Space;
    [SerializeField] KeyCode changeBulletKey = KeyCode.E;
    [SerializeField] BulletMode currentBullet = BulletMode.Damage;
    [SerializeField] GameObject bulletDamagePrefab;
    [SerializeField] GameObject bulletEnergyPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float timeBtwShoot = 0.5f;


    // Variables internas
    float moveSpeed = 0f;    // Velocidad actual de movimiento
    float rotateSpeed = 0f;  // Velocidad actual de rotacion
    float timer = 0f;        // timer que controla tiempos
    bool canShoot = true;
    

    Rigidbody2D rb;
    #endregion


    /// <summary>
    /// Inicializa referencias internas.
    /// Configura Rigidbody2D como Kinematic para movimiento controlado manualmente.
    /// </summary>
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

        HandleMovement(verticalInput);
        HandleRotation(horizontalInput);
    }

    #region Movimiento Player
    /// <summary>
    /// Maneja el movimiento del tanque usando Rigidbody2D.
    /// Usa MoveTowards para acelerar/desacelerar suavemente.
    /// Diferencia velocidad hacia adelante y hacia atras.
    /// </summary>
    void HandleMovement(float verticalInput)
    {
        float targetSpeed = 0f;

        if (verticalInput > 0)
        {
            targetSpeed = moveSpeedMax;
        }
        else if (verticalInput < 0)
        {
            targetSpeed = -moveSpeedMaxBackward;
        }

        float accel = (verticalInput != 0) ? moveAcceleration : moveDeceleration;

        moveSpeed = Mathf.MoveTowards(moveSpeed, targetSpeed, accel * Time.fixedDeltaTime);

        // Calcula la nueva posicion con MovePosition para fisica correcta
        Vector2 moveDirection = transform.up * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    /// <summary>
    /// Maneja la rotación usando Rigidbody2D.
    /// Aplica aceleración/desaceleración suave con MoveTowards.
    /// Usa Mathf.Sign para mantener la dirección clara y evitar inversión brusca.
    /// </summary>
    void HandleRotation(float horizontalInput)
    {
        float targetRotateSpeed = rotateSpeedMax * Mathf.Abs(horizontalInput);
        float accel = (horizontalInput != 0) ? rotateAcceleration : rotateDeceleration;

        rotateSpeed = Mathf.MoveTowards(rotateSpeed, targetRotateSpeed, accel * Time.fixedDeltaTime);

        float rotationAmount = -rotateSpeed * Mathf.Sign(horizontalInput) * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation + rotationAmount);
    }
    #endregion

    #region Disparo Player
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
    /// Cambia el tipo de bala al presionar la tecla definida.
    /// </summary>
    void HandleChangeBulletInput()
    {
        if (Input.GetKeyDown(changeBulletKey))
        {
            currentBullet = (currentBullet == BulletMode.Damage) ? BulletMode.Energy : BulletMode.Damage;
        }
    }
    #endregion
}

public enum BulletMode 
{ 
    Damage, 
    Energy 
}