using UnityEngine;
using System.Collections.Generic;

public class AutoDoor : MonoBehaviour
{
    #region Variables Generales
    [Header("Door Waypoints (relative offsets)")]
    public List<Vector2> waypoints = new List<Vector2>();

    [Header("Movement Settings")]
    public float speed = 2f;
    [Range(0, 2)] public float easeAmount = 1f;
    public float waitTime = 0.1f; // espera minima entre cambios de estado
    public float detectionRadius = 3f;

    Transform player;
    int targetIndex = 0;
    Vector2 initialPosition;
    float nextMoveTime = 0f;
    bool isOpen = false; // estado de la puerta
    float closeMargin = 0.3f; // histeresis para cerrar
    #endregion

    #region Unity Methods
    void Start()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("No hay waypoints definidos para la puerta.");
            return;
        }

        initialPosition = transform.position;
        transform.position = initialPosition + waypoints[0];
        targetIndex = 0;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || waypoints.Count == 0)
        {
            return;
        }

        // distancia relativa al primer waypoint (cerrado)
        float distance = Vector2.Distance(player.position, initialPosition + waypoints[0]);

        // comprobar si la puerta llego al objetivo
        Vector2 targetPos = initialPosition + waypoints[targetIndex];
        bool doorAtTarget = (Vector2)transform.position == targetPos;

        if (doorAtTarget && Time.time >= nextMoveTime)
        {
            if (!isOpen && distance <= detectionRadius)
            {
                isOpen = true;
                targetIndex = waypoints.Count - 1; // abrir
                nextMoveTime = Time.time + waitTime;
            }
            else if (isOpen && distance > detectionRadius + closeMargin)
            {
                isOpen = false;
                targetIndex = 0; // cerrar
                nextMoveTime = Time.time + waitTime;
            }
        }

        OpenDoor();
    }

    #endregion

    #region Door Movement
    /// <summary>
    /// Mueve la puerta hacia el waypoint objetivo de forma suave.
    /// </summary>
    void OpenDoor()
    {
        Vector2 targetPos = initialPosition + waypoints[targetIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }
    #endregion

    #region Gizmos Visualization
    /// <summary>
    /// Dibuja en el editor los waypoints de la puerta y las lineas de conexion.
    /// </summary>
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.red;
        float size = 0.2f;
        Vector2 basePos = Application.isPlaying ? initialPosition : (Vector2)transform.position;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector2 globalPos = basePos + waypoints[i];
            // Dibuja una cruz (+) en cada waypoint
            Gizmos.DrawLine(globalPos + Vector2.left * size, globalPos + Vector2.right * size);
            Gizmos.DrawLine(globalPos + Vector2.up * size, globalPos + Vector2.down * size);

            if (i > 0)
            {
                Vector2 prevGlobal = basePos + waypoints[i - 1];
                Gizmos.DrawLine(prevGlobal, globalPos);
            }
        }
    }
    #endregion
}
