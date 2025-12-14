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

    private bool isNotebookActive = false;
    private Coroutine activeAnimation;
    private bool pendingEndLevelUI = false;

    // --- MÉTODOS ESTÁNDAR DE UNITY ---

    void Start()
    {
        // 1. Posicionar en el Holster y desactivar al inicio.
        transform.position = holsterPoint.position;
        transform.rotation = holsterPoint.rotation;
        gameObject.SetActive(false);
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
                // CALLBACK: Después de desplegarse
                if (pendingEndLevelUI)
                {
                    Debug.Log("NotebookController: Notificando a UIManager para mostrar Panel de Fin de Nivel.");
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ActivateEndLevelUIPanel();
                    }
                    pendingEndLevelUI = false;
                }
            }));
            Debug.Log("NotebookController: Cuaderno desplegado.");
        }
        else
        {
            // Guardar:
            pendingEndLevelUI = false;

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
    /// Corrutina que anima el movimiento del cuaderno entre dos puntos.
    /// </summary>
    private IEnumerator AnimateMovement(Transform startPoint, Transform endPoint, Action onComplete = null) // <-- ¡ASEGÚRATE DE QUE ESTÉ PRESENTE!
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

    // --- FUNCIONES DE INTERFAZ PARA EL FLUJO DE JUEGO ---

    public void DeployAndShowMissionUI()
    {
        pendingEndLevelUI = false;

        if (!isNotebookActive)
        {
            ToggleNotebook();
        }
        Debug.Log("NotebookController: Solicitud de despliegue para UI de Misión.");
    }

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
    /// </summary>
    public void DeployAndShowEndLevelUI()
    {
        if (pendingEndLevelUI) return;

        if (isNotebookActive)
        {
            // Cuaderno ya activo: llama a la UI inmediatamente (sin esperar animación).
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ActivateEndLevelUIPanel();
            }
        }
        else
        {
            // Cuaderno guardado: inicia el despliegue y marca la bandera para el callback.
            pendingEndLevelUI = true;
            ToggleNotebook();
        }
    }
}