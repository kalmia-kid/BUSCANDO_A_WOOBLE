using UnityEngine;
using System.Collections;
using System;

public class NotebookController : MonoBehaviour
{
    // -------------------------------------------------------------------
    // Implementación de Singleton para fácil acceso
    // -------------------------------------------------------------------
    public static NotebookController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No usar DontDestroyOnLoad a menos que sea un Singleton de juego completo
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // -------------------------------------------------------------------

    [Header("Configuración de Puntos de Anclaje")]
    [Tooltip("El punto donde el cuaderno debe aparecer (GripPoint en la mano).")]
    public Transform gripPoint;
    [Tooltip("El punto donde el cuaderno 'descansa' (HolsterPoint en el cuerpo).")]
    public Transform holsterPoint;

    [Header("Parámetros de Animación")]
    [Tooltip("Tiempo que tarda la animación de deslizamiento (en segundos).")]
    public float animationDuration = 0.3f;

    // NOTA: Se eliminan las referencias a missionStartUIPanel y endLevelUIPanel 
    // porque el UIManager ahora maneja su visibilidad.

    private bool isNotebookActive = false;
    private Coroutine activeAnimation;

    // Nueva bandera: Indica si se debe notificar al UIManager para que muestre el panel 
    // de fin de nivel una vez que la animación de apertura termine.
    private bool pendingEndLevelUI = false;

    // --- MÉTODOS ESTÁNDAR DE UNITY ---

    void Start()
    {
        // 1. Posicionar en el Holster y desactivar al inicio.
        transform.position = holsterPoint.position;
        transform.rotation = holsterPoint.rotation;
        gameObject.SetActive(false); // Asegurarse de que el objeto físico esté oculto.

        // NOTA: La lógica de esconder los paneles de UI se movió al UIManager.Start()
    }

    // --- LÓGICA DE APARICIÓN/DESAPARICIÓN ---

    public bool IsActive()
    {
        return isNotebookActive;
    }

    /// <summary>
    /// Alterna la visibilidad del objeto físico del portafolio, gestionando la animación.
    /// </summary>
    public void ToggleNotebook()
    {
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }

        isNotebookActive = !isNotebookActive;

        if (isNotebookActive)
        {
            // Desplegar:
            gameObject.SetActive(true);
            activeAnimation = StartCoroutine(AnimateMovement(holsterPoint, gripPoint, () =>
            {
                // CALLBACK: Después de desplegarse, notifica al UIManager si hay una UI pendiente
                if (pendingEndLevelUI)
                {
                    // Llama al UIManager (asumiendo que tiene un Singleton o una referencia estática)
                    // para mostrar el panel que estaba esperando la animación.
                    // Aquí NO hacemos la activación, solo notificamos.
                    // El UIManager se encargará de hacer EndLevelUIPanel.SetActive(true) y ocultar otros paneles.
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ActivateEndLevelUIPanel();
                    }
                    pendingEndLevelUI = false; // La notificación ya ha sido enviada
                }
            }));
            Debug.Log("NotebookController: Cuaderno desplegado.");
        }
        else
        {
            // Guardar:

            // NO TOCAMOS NINGÚN PANEL DE UI AQUÍ.
            // La ocultación de todos los paneles (si es necesario) debe ser llamada
            // antes de ToggleNotebook() por la función que inicia el guardado (ej: HideAllUIAndResumeGame).

            pendingEndLevelUI = false; // Cancelamos cualquier UI pendiente.

            // Inicia la animación de guardado, y desactiva el objeto al completarse.
            activeAnimation = StartCoroutine(AnimateMovement(gripPoint, holsterPoint, () =>
            {
                // Callback: Se ejecuta al terminar la animación de guardado.
                gameObject.SetActive(false);
            }));
            Debug.Log("NotebookController: Cuaderno guardado.");
        }
    }

    private IEnumerator AnimateMovement(Transform startPoint, Transform endPoint, Action onComplete = null)
    {
        float elapsedTime = 0f;

        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            transform.rotation = Quaternion.Lerp(startPoint.rotation, endPoint.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPoint.position;
        transform.rotation = endPoint.rotation;

        onComplete?.Invoke();

        activeAnimation = null;
    }

    // --- FUNCIONES DE INTERFAZ PARA EL FLUJO DE JUEGO (llamadas desde el GameManager o UIManager) ---

    /// <summary>
    /// Despliega el cuaderno y notifica al UIManager para mostrar el panel de Misión.
    /// Llamada por el GameManager al inicio del nivel.
    /// </summary>
    public void DeployAndShowMissionUI()
    {
        // NOTA: El UIManager DEBE asegurarse de que el panel de Misión esté visible 
        // antes o justo después de llamar a esta función.

        if (!isNotebookActive)
        {
            // Despliega el cuaderno (ToggleNotebook se encarga de la animación de apertura).
            ToggleNotebook();
        }

        Debug.Log("NotebookController: Solicitud de despliegue para UI de Misión.");
    }

    /// <summary>
    /// Guarda el cuaderno. Llamada por el botón 'Empezar Misión' en el MissionStart_Panel (a través del UIManager).
    /// </summary>
    public void HideNotebook()
    {
        if (isNotebookActive)
        {
            ToggleNotebook();
        }
        Debug.Log("NotebookController: Cuaderno escondido.");
    }


    /// <summary>
    /// Prepara el cuaderno para mostrar el panel de opciones de fin de nivel.
    /// Llamada por el GameManager cuando el nivel ha terminado.
    /// </summary>
    public void DeployAndShowEndLevelUI()
    {
        // Evita doble activación
        if (pendingEndLevelUI) return;

        // Si el cuaderno ya está activo, la UI puede aparecer inmediatamente.
        if (isNotebookActive)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ActivateEndLevelUIPanel();
            }
        }
        else
        {
            // Si el cuaderno está guardado, marca la bandera y usa ToggleNotebook().
            // El callback de ToggleNotebook() llamará a UIManager.Instance.ActivateEndLevelUIPanel().
            pendingEndLevelUI = true;
            gameObject.SetActive(true);
            ToggleNotebook();
        }
    }
}