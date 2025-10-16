using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float maxForwardSpeed = 3f;
    [SerializeField] float maxBackwardSpeed = 1.5f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float deceleration = 5f;

    [Header("Rotation")]
    [SerializeField] float maxRotateSpeed = 150f;
    [SerializeField] float rotateAcceleration = 400f;
    [SerializeField] float rotateDeceleration = 400f;

    [Header("Impact")]
    [SerializeField] float knockbackDecay = 8f;

    Rigidbody2D rb;
    float currentMoveSpeed;
    float currentRotateSpeed;
    Vector2 impactVelocity = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void FixedUpdate()
    {
        ProcessMovementInput();
        ProcessRotationInput();
        UpdateKnockback();
    }

    #region Movement Methods
    /// <summary>
    /// Calcula la velocidad objetivo según input vertical.
    /// </summary>
    float CalculateTargetSpeed(float verticalInput)
    {
        if (verticalInput > 0f) return maxForwardSpeed;
        if (verticalInput < 0f) return -maxBackwardSpeed;
        return 0f;
    }

    /// <summary>
    /// Aplica movimiento al Rigidbody2D del jugador.
    /// </summary>
    void ApplyMovement(float speed)
    {
        // Movimiento en la dirección del tanque
        Vector2 moveDir = transform.up * currentMoveSpeed;

        // Aplicar knockback/impacto sumando a la velocidad
        rb.linearVelocity = moveDir + impactVelocity;
    }

    /// <summary>
    /// Procesa el input vertical y aplica aceleración/desaceleración.
    /// </summary>
    void ProcessMovementInput()
    {
        float vertical = Input.GetAxis("Vertical");

        float targetSpeed = CalculateTargetSpeed(vertical);
        float accel = (vertical != 0f) ? acceleration : deceleration;

        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, accel * Time.fixedDeltaTime);

        ApplyMovement(currentMoveSpeed);
    }
    #endregion

    #region Rotation Methods
    /// <summary>
    /// Calcula la velocidad de rotación objetivo según input horizontal.
    /// </summary>
    float CalculateTargetRotationSpeed(float horizontalInput)
    {
        return maxRotateSpeed * Mathf.Abs(horizontalInput);
    }

    /// <summary>
    /// Aplica rotación al Rigidbody2D del jugador.
    /// </summary>
    void ApplyRotation(float rotationSpeed, float horizontalInput)
    {
        rb.MoveRotation(rb.rotation - rotationSpeed * Mathf.Sign(horizontalInput) * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Procesa el input horizontal y aplica aceleración/desaceleración de rotación.
    /// </summary>
    void ProcessRotationInput()
    {
        float horizontal = Input.GetAxis("Horizontal");

        float targetRotateSpeed = CalculateTargetRotationSpeed(horizontal);
        float accel = (horizontal != 0f) ? rotateAcceleration : rotateDeceleration;

        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotateSpeed, accel * Time.fixedDeltaTime);

        ApplyRotation(currentRotateSpeed, horizontal);
    }
    #endregion

    #region Knockback / Impact Methods
    /// <summary>
    /// Reduce gradualmente la velocidad del knockback.
    /// </summary>
    void UpdateKnockback()
    {
        impactVelocity = Vector2.Lerp(impactVelocity, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Aplica knockback externo al jugador (daño, explosión, etc.)
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        impactVelocity += direction.normalized * force;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Devuelve la velocidad actual del jugador.
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentMoveSpeed;
    }

    public float GetCurrentRotateSpeed()
    {
        return currentRotateSpeed;
    }
    #endregion
}
