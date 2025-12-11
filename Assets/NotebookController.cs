using UnityEngine;
using System.Collections;
using System; // Necesario para System.Action

public class NotebookController : MonoBehaviour
{
    [Header("Configuración de Puntos de Anclaje")]
    [Tooltip("El punto donde el cuaderno debe aparecer (GripPoint en la mano).")]
    public Transform gripPoint;
    [Tooltip("El punto donde el cuaderno 'descansa' (HolsterPoint en el cuerpo).")]
    public Transform holsterPoint;

    [Header("Parámetros de Animación")]
    [Tooltip("Tiempo que tarda la animación de deslizamiento (en segundos).")]
    public float animationDuration = 0.3f;

    [Header("UI y Botones")]
    [Tooltip("El GameObject que contiene los botones y el texto de Fin de Nivel.")]
    public GameObject endLevelUIPanel;

    private bool isNotebookActive = false;
    private Coroutine activeAnimation;

    // --- MÉTODOS ESTÁNDAR DE UNITY ---

    void Start()
    {
        // 1. Posicionar en el Holster y desactivar al inicio.
        transform.position = holsterPoint.position;
        transform.rotation = holsterPoint.rotation;
        gameObject.SetActive(false);

        // 2. Asegurarse de que el panel de UI de fin de nivel esté escondido al inicio.
        if (endLevelUIPanel != null)
        {
            endLevelUIPanel.SetActive(false);
        }
    }

    // --- LÓGICA DE APARICIÓN/DESAPARICIÓN ---

    /// <summary>
    /// Devuelve el estado actual del cuaderno.
    /// </summary>
    public bool IsActive()
    {
        return isNotebookActive;
    }

    /// <summary>
    /// Alterna el estado del cuaderno (Desplegar/Guardar).
    /// Esta función se conecta a la Input Action de RV del jugador.
    /// </summary>
    public void ToggleNotebook()
    {
        // Si ya hay una animación en curso, detenla antes de iniciar una nueva.
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }

        isNotebookActive = !isNotebookActive;

        if (isNotebookActive)
        {
            // Desplegar: 
            // Asegura que el GameObject esté activo antes de animar.
            gameObject.SetActive(true);
            activeAnimation = StartCoroutine(AnimateMovement(holsterPoint, gripPoint));
            Debug.Log("NotebookController: Cuaderno desplegado.");
        }
        else
        {
            // Guardar: 
            // Esconde la UI de Fin de Nivel inmediatamente si está visible.
            if (endLevelUIPanel != null)
            {
                endLevelUIPanel.SetActive(false);
            }
            // Inicia la animación de guardado, y desactiva el objeto al completarse.
            activeAnimation = StartCoroutine(AnimateMovement(gripPoint, holsterPoint, () =>
            {
                // Callback: Se ejecuta al terminar la animación de guardado.
                gameObject.SetActive(false);
            }));
            Debug.Log("NotebookController: Cuaderno guardado.");
        }
    }

    /// <summary>
    /// Corrutina para animar la posición y rotación del cuaderno.
    /// </summary>
    private IEnumerator AnimateMovement(Transform startPoint, Transform endPoint, Action onComplete = null)
    {
        float elapsedTime = 0f;

        // Establecer la posición inicial
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            // Usar SmoothStep para un movimiento más orgánico (empieza lento, rápido en el medio, termina lento)
            t = Mathf.SmoothStep(0f, 1f, t);

            // Interpolación de posición y rotación
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            transform.rotation = Quaternion.Lerp(startPoint.rotation, endPoint.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Espera un frame
        }

        // Asegura la posición final exacta
        transform.position = endPoint.position;
        transform.rotation = endPoint.rotation;

        // Ejecuta la función de completado (si existe)
        onComplete?.Invoke();

        activeAnimation = null; // Finaliza la corrutina
    }

    // --- FUNCIÓN DE FIN DE NIVEL (LLAMADA POR EL GAMEMANAGER) ---

    /// <summary>
    /// Muestra el cuaderno y el panel de opciones de fin de nivel.
    /// Llamada por el GameManager cuando Wooble es capturado definitivamente.
    /// </summary>
    public void ShowEndLevelUI()
    {
        // 1. Despliega el cuaderno si no está activo
        if (!isNotebookActive)
        {
            // Al hacer Toggle, se activará el objeto y la animación de despliegue.
            ToggleNotebook();
        }

        // 2. Activa la UI de botones
        if (endLevelUIPanel != null)
        {
            // Podríamos usar una Corrutina con un pequeño delay aquí para que la UI no aparezca 
            // hasta que el cuaderno esté completamente desplegado (al final del Lerp).
            endLevelUIPanel.SetActive(true);
            Debug.Log("NotebookController: UI de opciones de fin de nivel activada.");
        }
    }
}