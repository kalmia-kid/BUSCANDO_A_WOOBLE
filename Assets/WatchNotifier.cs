using UnityEngine;
using TMPro; // Asegúrate de tener el paquete TextMeshPro instalado
using System.Collections;

public class WatchNotifier : MonoBehaviour
{
    public static WatchNotifier Instance; // Singleton simple para acceso

    [Tooltip("El componente TextMeshPro que muestra el mensaje.")]
    public TextMeshPro messageText;

    [Tooltip("El tiempo que el mensaje de alarma permanece visible.")]
    public float displayDuration = 3f;

    void Awake()
    {
        // Implementación Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Asegurarse de que el texto no es visible al inicio.
        if (messageText != null)
        {
            messageText.text = "";
        }
        // este mensaje es temporal, hay que hacer una lista con los diferentes mensajes 
        string message = "Wooble ha escapado, atrápalo!";
        DisplayAlarm(message);
    } 

    /// <summary>
    /// Muestra un mensaje de alarma (llamada por GameManager/Wooble).
    /// </summary>
    public void DisplayAlarm(string message)
    {
        if (messageText != null)
        {
            StartCoroutine(ShowAndHideAlarm(message));

            // Aquí llamarías a la vibración del controlador:
            // InputDevice device = InputDevices.GetDeviceAtxrInteractionHand(InputController.RightHand);
            // HapticCapabilities capabilities;
            // if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            // {
            //     device.SendHapticImpulse(0, 0.5f, 0.2f); // Patrón de vibración corta
            // }
        }
    }

    private IEnumerator ShowAndHideAlarm(string message)
    {
        // Muestra el mensaje de alarma
        messageText.text = message;

        yield return new WaitForSeconds(displayDuration);

        // Limpia el mensaje de alarma
        messageText.text = "";
        messageText.color = Color.white;
    }
}