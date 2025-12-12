using UnityEngine;
using System.Collections;
using System; // Necesario para System.Action

public class NotebookController : MonoBehaviour
{
    // -------------------------------------------------------------------
    // AÑADIDO: Implementación de Singleton para fácil acceso
    // -------------------------------------------------------------------
    public static NotebookController Instance { get; private set; }

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
    // -------------------------------------------------------------------

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
    private bool uiIsPending = false; // Bandera para saber si la UI debe activarse

    // --- MÉTODOS ESTÁNDAR DE UNITY ---

    void Start()
    {
        // 1. Posicionar en el Holster y desactivar al inicio.
        transform.position = holsterPoint.position;
        transform.rotation = holsterPoint.rotation;
        // Se mantiene activo si ya está en la escena, pero se mueve.
        // gameObject.SetActive(false); // <--- Lo activará ToggleNotebook si es necesario

        // 2. Asegurarse de que el panel de UI de fin de nivel esté escondido al inicio.
        if (endLevelUIPanel != null)
        {
            endLevelUIPanel.SetActive(false);
        }
    }

    // --- LÓGICA DE APARICIÓN/DESAPARICIÓN ---

    public bool IsActive()
    {
        return isNotebookActive;
    }

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
                // CALLBACK: Después de desplegarse, verifica si la UI de fin de nivel está pendiente
                if (uiIsPending)
                {
                    ActivateEndLevelUI();
                }
            }));
            Debug.Log("NotebookController: Cuaderno desplegado.");
        }
        else
        {
            // Guardar:
            // Asegúrate de desactivar la bandera de UI pendiente si el jugador lo cierra
            uiIsPending = false;

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

        // Ejecuta la función de completado (si existe)
        onComplete?.Invoke();

        activeAnimation = null;
    }

    // --- FUNCIÓN DE FIN DE NIVEL (LÓGICA CENTRAL) ---

    private void ActivateEndLevelUI()
    {
        if (endLevelUIPanel != null)
        {
            endLevelUIPanel.SetActive(true);
            uiIsPending = false; // La UI ya no está pendiente
            Debug.Log("NotebookController: UI de opciones de fin de nivel ACTIVADA.");
            // Opcional: Time.timeScale = 0f; si quieres pausar el juego
        }
    }

    /// <summary>
    /// Muestra el cuaderno y el panel de opciones de fin de nivel.
    /// Llamada por el GameManager cuando Wooble es capturado definitivamente.
    /// </summary>
    public void ShowEndLevelUI()
    {
        if (endLevelUIPanel.activeSelf) return; // Ya está mostrando la UI

        // 1. Si el cuaderno ya está activo, muestra la UI inmediatamente.
        if (isNotebookActive)
        {
            ActivateEndLevelUI();
        }
        else
        {
            // 2. Si el cuaderno está guardado, marca la bandera y usa ToggleNotebook()
            uiIsPending = true;
            gameObject.SetActive(true); // Asegura que el objeto esté activo para que Toggle pueda animar
            ToggleNotebook();
        }
    }
}