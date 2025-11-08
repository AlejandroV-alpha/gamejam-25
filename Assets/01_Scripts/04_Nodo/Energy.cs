using UnityEngine;

/// <summary>
/// Componente que transfiere una cantidad definida de energia a otro objeto
/// que implemente la interfaz ITakeEnergy al detectar una colision tipo Trigger.
/// Tambien se autodestruye despues de un tiempo definido.
/// </summary>
public class Energy : MonoBehaviour
{
    [SerializeField] float energy = 5f;
    [SerializeField] float timeToDestroy = 10f;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    /// <summary>
    /// Se ejecuta automaticamente cuando otro Collider2D entra en contacto con este objeto.
    /// Si el objeto colisionado implementa ITakeEnergy, se le transfiere la energia definida.
    /// Luego, este objeto se destruye inmediatamente.
    /// </summary>
    /// <param name="collision">El collider del objeto que entra en contacto.</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        ITakeEnergy takeEnergy = collision.GetComponent<ITakeEnergy>();
        if (takeEnergy != null && collision.gameObject.CompareTag("Player"))
        {
            takeEnergy.TakeEnergy(energy);
            Debug.Log($"Energia transferida: {energy}");
            Destroy(gameObject);
        }
    }
}