using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Tracks (Animation)")]
    [SerializeField] Animator leftTrack;    
    [SerializeField] Animator rightTrack;    
    [SerializeField] float maxLinearSpeed = 6f;  
    [SerializeField] float turnInfluence = 0.003f; 
    [SerializeField] bool invertOnReverse = true;  

    static readonly int MovingHash = Animator.StringToHash("Moving");
    static readonly int SpeedMultHash = Animator.StringToHash("SpeedMult");
  
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

    float baseMaxForwardSpeed;
    float baseMaxBackwardSpeed;
    float baseMaxRotateSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        baseMaxForwardSpeed = maxForwardSpeed;
        baseMaxBackwardSpeed = maxBackwardSpeed;
        baseMaxRotateSpeed = maxRotateSpeed;

        if (!leftTrack)
        {
            var t = transform.Find("Graphics/TankLeft");
            if (t) leftTrack = t.GetComponent<Animator>();
        }
        if (!rightTrack)
        {
            var t = transform.Find("Graphics/TankRight");
            if (t) rightTrack = t.GetComponent<Animator>();
        }
        if (maxLinearSpeed <= 0f) maxLinearSpeed = 1f;
    }

    void FixedUpdate()
    {
        ProcessMovementInput();
        ProcessRotationInput();
        UpdateKnockback();
        UpdateTrackAnim();
    }

    #region Movement Methods
    /// <summary>
    /// Calcula la velocidad objetivo seg�n input vertical.
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
        // Movimiento en la direcci�n del tanque
        Vector2 moveDir = transform.up * currentMoveSpeed;

        // Aplicar knockback/impacto sumando a la velocidad
        rb.linearVelocity = moveDir + impactVelocity;
    }

    /// <summary>
    /// Procesa el input vertical y aplica aceleraci�n/desaceleraci�n.
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
    /// Calcula la velocidad de rotaci�n objetivo seg�n input horizontal.
    /// </summary>
    float CalculateTargetRotationSpeed(float horizontalInput)
    {
        return maxRotateSpeed * Mathf.Abs(horizontalInput);
    }

    /// <summary>
    /// Aplica rotaci�n al Rigidbody2D del jugador.
    /// </summary>
    void ApplyRotation(float rotationSpeed, float horizontalInput)
    {
        rb.MoveRotation(rb.rotation - rotationSpeed * Mathf.Sign(horizontalInput) * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Procesa el input horizontal y aplica aceleraci�n/desaceleraci�n de rotaci�n.
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
    /// Aplica knockback externo al jugador (da�o, explosi�n, etc.)
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

    #region PlayerStatus
    /// <summary>
    /// Multiplica las velocidades m�ximas por el factor indicado
    /// </summary>
    public void ModifySpeed(float multiplier)
    {
        maxForwardSpeed = baseMaxForwardSpeed * multiplier;
        maxBackwardSpeed = baseMaxBackwardSpeed * multiplier;
        maxRotateSpeed = baseMaxRotateSpeed * multiplier;
    }
    #endregion

    #region Animation
    void UpdateTrackAnim()
    {
        // velocidad real del RB (usa velocity, no linearVelocity)
        Vector2 v = rb.linearVelocity;

        // componente de la velocidad en el �frente� del tanque
        float forward = Vector2.Dot(v, (Vector2)transform.up);

        float movingAmount = Mathf.Abs(forward);
        bool isMoving = movingAmount > 0.05f || Mathf.Abs(rb.angularVelocity) > 5f;

        // 0..1 seg�n rapidez
        float linMult = Mathf.Clamp01(movingAmount / maxLinearSpeed);

        // direcci�n (signo) para reversa
        float dir = invertOnReverse ? Mathf.Sign(forward) : 1f;

        // diferencial por giro (una rueda avanza m�s que la otra)
        float diff = rb.angularVelocity * turnInfluence;

        SetTrack(leftTrack, isMoving, dir * linMult + diff);
        SetTrack(rightTrack, isMoving, dir * linMult - diff);
    }

    void SetTrack(Animator a, bool moving, float mult)
    {
        if (!a) return;
        a.SetBool(MovingHash, moving);
        a.SetFloat(SpeedMultHash, mult);
    }
    #endregion
}
