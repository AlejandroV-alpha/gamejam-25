using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float moveSpeedMax = 2.5f;         // Velocidad maxima hacia adelante
    [SerializeField] float moveSpeedMaxBackward = 1.5f; // Velocidad maxima hacia atras
    [SerializeField] float moveAcceleration = 5f;       // Velocidad de aceleracion
    [SerializeField] float moveDeceleration = 5f;       // Velocidad de desaceleracion

    [Header("Rotacion")]
    [SerializeField] float rotateSpeedMax = 130f;       // Velocidad maxima de rotacion
    [SerializeField] float rotateAcceleration = 400f;   // Aceleracion de rotacion
    [SerializeField] float rotateDeceleration = 400f;   // Desaceleracion de rotacion

    // Variables internas
    float moveSpeed = 0f;    // Velocidad actual de movimiento
    float rotateSpeed = 0f;  // Velocidad actual de rotacion

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Lo hacemos Kinematic para controlar el movimiento manualmente
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void FixedUpdate()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        HandleMovement(verticalInput);
        HandleRotation(horizontalInput);
    }

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
    /// Maneja la rotacion usando Rigidbody2D.
    /// MoveTowards para aceleracion/desaceleracion suave.
    /// Se multiplica por -1 para que coincida la direccion con el input horizontal.
    /// </summary>
    void HandleRotation(float horizontalInput)
    {
        float targetRotateSpeed = (horizontalInput != 0) ? rotateSpeedMax : 0f;
        float accel = (horizontalInput != 0) ? rotateAcceleration : rotateDeceleration;

        rotateSpeed = Mathf.MoveTowards(rotateSpeed, targetRotateSpeed, accel * Time.fixedDeltaTime);

        float rotationAmount = -rotateSpeed * horizontalInput * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }
}