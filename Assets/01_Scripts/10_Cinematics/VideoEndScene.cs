using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoEndScene : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string nombreEscenaSiguiente = ""; // Cambia al nombre de tu escena

    VideoPlayer videoPlayer;

    void Start()
    {
        // Obtener el componente VideoPlayer del mismo objeto
        videoPlayer = GetComponent<VideoPlayer>();

        // Suscribirse al evento de fin de video
        videoPlayer.loopPointReached += AfterVideo;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nombreEscenaSiguiente);
        }
    }

    void AfterVideo(VideoPlayer vp)
    {
        // Cargar la siguiente escena
        SceneManager.LoadScene(nombreEscenaSiguiente);
    }
}
