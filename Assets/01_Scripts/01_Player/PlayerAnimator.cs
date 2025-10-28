using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    #region Variables
    [Header("Tracks (Animation)")]
    [SerializeField] Animator leftTrack;
    [SerializeField] Animator rightTrack;

    [Header("Movement Settings")]
    [SerializeField] float maxLinearSpeed = 6f;
    [SerializeField] float turnInfluence = 0.015f;
    [SerializeField] bool invertOnReverse = true;

    static readonly int MovingHash = Animator.StringToHash("IsMoving");
    static readonly int SpeedMultHash = Animator.StringToHash("SpeedMult");

    Rigidbody2D rb;
    PlayerController playerController;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Inicializa referencias y verifica componentes
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (!leftTrack)
        {
            var t = transform.Find("Graphics/TankLeft");
            if (t)
            {
                leftTrack = t.GetComponent<Animator>();
            }
        }

        if (!rightTrack)
        {
            var t = transform.Find("Graphics/TankRight");
            if (t)
            {
                rightTrack = t.GetComponent<Animator>();
            }
        }

        if (maxLinearSpeed <= 0f)
        {
            maxLinearSpeed = 1f;
        }
    }

    /// <summary>
    /// Actualiza animaciones cada FixedUpdate
    /// </summary>
    void FixedUpdate()
    {
        UpdateTrackAnim();
    }
    #endregion

    #region Animation Methods
    /// <summary>
    /// Actualiza animacion de las orugas segun velocidad lineal y rotacion
    /// </summary>
    void UpdateTrackAnim()
    {
        float forward = Vector2.Dot(rb.linearVelocity, transform.up);
        float movingAmount = Mathf.Abs(forward);

        float rotationVel = playerController ? playerController.GetCurrentRotateSpeed() : 0f;
        bool isMoving = movingAmount > 0.05f || Mathf.Abs(rotationVel) > 0.1f;

        float linMult = Mathf.Clamp01(movingAmount / maxLinearSpeed);
        float dir = invertOnReverse ? Mathf.Sign(forward) : 1f;

        // Correccion: signo de rotationVel ya esta incluido
        float leftSpeed = dir * linMult - rotationVel * turnInfluence;
        float rightSpeed = dir * linMult + rotationVel * turnInfluence;

        leftSpeed = Mathf.Clamp(leftSpeed, -1f, 1f);
        rightSpeed = Mathf.Clamp(rightSpeed, -1f, 1f);

        SetTrack(leftTrack, isMoving, leftSpeed);
        SetTrack(rightTrack, isMoving, rightSpeed);
    }

    /// <summary>
    /// Configura la animacion de un track individual
    /// </summary>
    /// <param name="animator">Animator del track</param>
    /// <param name="moving">Indica si se esta moviendo</param>
    /// <param name="speedMult">Multiplicador de velocidad</param>
    void SetTrack(Animator animator, bool moving, float speedMult)
    {
        if (!animator)
        {
            return;
        }

        animator.SetBool(MovingHash, moving);
        animator.SetFloat(SpeedMultHash, speedMult);
    }
    #endregion
}
