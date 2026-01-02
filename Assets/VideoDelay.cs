using UnityEngine;
using UnityEngine.Video; // Necesario para controlar video
using System.Collections;

public class DelayedVideoStarter : MonoBehaviour
{
    [Tooltip("Arrastra aquí el componente Video Player")]
    public VideoPlayer videoPlayer;

    [Tooltip("Tiempo de espera en segundos")]
    public float delayTime = 3.0f;

    void Start()
    {
        // Iniciamos la rutina de espera
        StartCoroutine(PlayVideoRoutine());
    }

    IEnumerator PlayVideoRoutine()
    {
        // Opcional: Preparar el video para que cargue en memoria mientras espera
        // videoPlayer.Prepare(); 
        
        // Esperar los segundos definidos
        yield return new WaitForSeconds(delayTime);

        // Reproducir
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("¡No has asignado el Video Player en el inspector!");
        }
    }
}