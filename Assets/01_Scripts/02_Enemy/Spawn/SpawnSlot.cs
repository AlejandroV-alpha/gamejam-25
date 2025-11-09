using UnityEngine;

/// <summary>
/// Clase que representa un punto de spawn individual y su temporizador.
/// </summary>
public class SpawnSlot
{
    public Transform spawnPoint;
    public GameObject currentEnemy;
    public float timer;
    public bool waitingRespawn;

    public SpawnSlot(Transform point)
    {
        spawnPoint = point;
        currentEnemy = null;
        timer = 0f;
        waitingRespawn = false;
    }
}
