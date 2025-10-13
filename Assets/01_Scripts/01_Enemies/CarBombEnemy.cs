using UnityEngine;
using UnityEngine.AI;

public class TankMover : MonoBehaviour
{
    public Transform destino; // Asigná el destino desde el Inspector
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.updateRotation = false;
        agente.updateUpAxis = false;

        if (destino != null)
        {
            agente.SetDestination(destino.position);
        }
    }

    void Update()
    {
        // Si querés que se actualice dinámicamente
        if (destino != null)
        {
            agente.SetDestination(destino.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ITakeDamage p = collision.gameObject.GetComponent<Player>();
            if (p != null)
            {
                Vector2 hitDir = (collision.transform.position - transform.position).normalized;

                p.TakeDamage(1, hitDir, 10);
                Destroy(gameObject);
            }
        }
    }
}