// RotatorTowardsTarget.cs
using UnityEngine;

/// <summary>
/// Componente que rota gradualmente hacia un objetivo.
/// Reutilizable en torretas y enemigos que apuntan al jugador.
/// </summary>
public class RotatorTowardsTarget : MonoBehaviour
{
    #region Inspector
    [SerializeField] float rotateSpeed = 120f;
    [SerializeField] float rotationOffset = -90f;
    #endregion

    #region Private Fields
    Transform partToRotate;
    Transform target;
    Quaternion initialRotation;
    #endregion

    #region Public Properties
    public Transform CurrentTarget()
    {
        return target;
    }
    #endregion

    #region Public Methods
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetPartToRotate(Transform part)
    {
        partToRotate = part;
        if (part != null)
        {
            initialRotation = part.localRotation;
        }
    }

    public void Tick()
    {
        if (partToRotate == null)
        {
            return;
        }

        Quaternion targetRotation;

        if (target != null)
        {
            Vector2 direction = (target.position - partToRotate.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            targetRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            targetRotation = initialRotation;
        }

        partToRotate.rotation = Quaternion.RotateTowards(
            partToRotate.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void ResetRotation()
    {
        target = null;
        Tick();
    }
    #endregion
}
