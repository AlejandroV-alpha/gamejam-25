using UnityEngine;

public class PropellerRotation : MonoBehaviour
{
    [SerializeField] private float speed = 600f;
    [SerializeField] private bool reverse = false;

    public float Speed { get => speed; set => speed = value; } // por si luego quieres controlarla

    void Update()
    {
        float dir = reverse ? -1f : 1f;
        transform.Rotate(0f, 0f, dir * speed * Time.deltaTime);
    }
}
