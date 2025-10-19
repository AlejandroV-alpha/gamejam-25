using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerStatus : MonoBehaviour, IStunnable, ISlowable
{
    PlayerController playerController;

    #region Unity Methods
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }
    #endregion

    #region IStunnable
    public void Stun(float duration)
    {
        Debug.Log("Aturdido por electricidad");
        StartCoroutine(StunCoroutine(duration));
    }

    IEnumerator StunCoroutine(float duration)
    {
        playerController.enabled = false;

        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);
        playerController.enabled = true;
    }
    #endregion

    #region ISlowable
    Coroutine slowCoroutine;

    /// <summary>
    /// Aplica ralentización: multiplica la velocidad actual por speedMultiplier durante duration segundos
    /// </summary>
    /// <param name="duration">Duración del efecto</param>
    /// <param name="speedMultiplier">Multiplicador de velocidad (0.6 = 40% más lento)</param>
    public void Slow(float duration, float speedMultiplier)
    {
        // Si ya hay un efecto activo, lo reiniciamos con la nueva duración y factor
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
           
        StartCoroutine(SlowCoroutine(duration, speedMultiplier));
    }

    IEnumerator SlowCoroutine(float duration, float speedMultiplier)
    {
        playerController.ModifySpeed(speedMultiplier);
        yield return new WaitForSeconds(duration);
        playerController.ModifySpeed(1f); // vuelve a la velocidad normal
        slowCoroutine = null;
    }
    #endregion
}

