using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Collections.Generic;

public class WatchNotifier : MonoBehaviour
{
    public static WatchNotifier Instance;

    [Header("Configuración UI")]
    [Tooltip("El componente TextMeshPro que muestra el mensaje.")]
    public TextMeshPro messageText;

    [Tooltip("El tiempo que CADA mensaje permanece visible.")]
    public float displayDuration = 3f;

    [Tooltip("Tiempo de espera entre un mensaje y el siguiente (si hay varios).")]
    public float delayBetweenMessages = 1.0f; // <--- NUEVA VARIABLE

    [Header("Configuración de Vibración (Haptics)")]
    [Tooltip("¿En qué mano está el reloj?")]
    public InputDeviceCharacteristics controllerHand = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;

    [Range(0, 1)]
    [Tooltip("Fuerza de la vibración.")]
    public float vibrationIntensity = 0.5f;

    [Tooltip("Duración de la vibración.")]
    public float vibrationDuration = 0.5f;

    void Awake()
    {
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
        if (messageText != null)
        {
            messageText.text = "";
        }

        DetermineAndShowMessage();
    }

    private void DetermineAndShowMessage()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Creamos una lista para guardar los mensajes que queremos mostrar en orden
        List<string> messagesSequence = new List<string>();

        // LÓGICA DE NIVELES
        if (currentSceneIndex == 0) // Nivel 1
        {
            // Añadimos los dos mensajes en orden
            messagesSequence.Add("Alerta 301: ¡Wooble ha escapado, atrápalo!");
            messagesSequence.Add("No olvides usar el AtrapaAliens4000");
        }
        else // Resto de niveles
        {
            messagesSequence.Add("Alerta 307: ¡Wooble se ha vuelto a escapar, atrápalo!");
        }

        // Llamamos a la nueva función que acepta una lista
        DisplaySequence(messagesSequence);
    }

    /// <summary>
    /// Muestra una secuencia de mensajes uno tras otro.
    /// </summary>
    public void DisplaySequence(List<string> messages)
    {
        if (messageText != null && messages.Count > 0)
        {
            // Detenemos cualquier mensaje anterior para evitar superposiciones
            StopAllCoroutines();
            StartCoroutine(ShowSequenceRoutine(messages));
        }
    }

    /// <summary>
    /// Mantiene la compatibilidad por si otros scripts llaman a DisplayAlarm con un solo texto.
    /// </summary>
    public void DisplayAlarm(string singleMessage)
    {
        List<string> singleList = new List<string> { singleMessage };
        DisplaySequence(singleList);
    }

    /// <summary>
    /// Corrutina que recorre la lista y muestra los mensajes paso a paso.
    /// </summary>
    private IEnumerator ShowSequenceRoutine(List<string> messages)
    {
        // Recorremos cada mensaje de la lista
        foreach (string msg in messages)
        {
            // 1. Poner texto
            messageText.text = msg;

            // 2. Vibrar (Vibra cada vez que aparece un mensaje nuevo)
            TriggerHapticFeedback();

            // 3. Esperar el tiempo de lectura
            yield return new WaitForSeconds(displayDuration);

            // 4. Limpiar texto
            messageText.text = "";

            // 5. Pequeña pausa antes del siguiente mensaje (si lo hay)
            yield return new WaitForSeconds(delayBetweenMessages);
        }
    }

    private void TriggerHapticFeedback()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerHand, devices);

        foreach (var device in devices)
        {
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                device.SendHapticImpulse(0u, vibrationIntensity, vibrationDuration);
                // Debug.Log($"WatchNotifier: Vibración enviada a {device.name}");
            }
        }
    }
}