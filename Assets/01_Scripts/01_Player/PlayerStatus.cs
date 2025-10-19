using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerStatus : MonoBehaviour, IStunnable
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
}

