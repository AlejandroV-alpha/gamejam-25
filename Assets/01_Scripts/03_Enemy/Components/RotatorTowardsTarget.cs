using UnityEngine;

/// <summary>
/// Componente que rota suavemente un objeto hacia un objetivo.
/// Si no hay objetivo, vuelve a su rotacion inicial.
/// </summary>
public class RotatorTowardsTarget : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] float rotateSpeed = 180f;
    #endregion

    #region Private Fields
    Transform target;
    Quaternion initialRotation;
    #endregion

    #region Unity Methods
    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (target != null)
        {
            RotateTowards(target);
        }
        else
        {
            ReturnToInitialRotation();
        }
    }
    #endregion

    #region Rotation Logic
    /// <summary>
    /// Rota suavemente hacia el objetivo.
    /// </summary>
    /// <param name="target">Transform del objetivo.</param>
    public void RotateTowards(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRot = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Rota suavemente hacia la rotacion inicial.
    /// </summary>
    void ReturnToInitialRotation()
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            initialRotation,
            rotateSpeed * Time.deltaTime
        );
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Asigna el objetivo al que rotar.
    /// </summary>
    /// <param name="newTarget">Transform del nuevo objetivo.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Elimina el objetivo actual.
    /// </summary>
    public void ClearTarget()
    {
        target = null;
    }
    #endregion
}
