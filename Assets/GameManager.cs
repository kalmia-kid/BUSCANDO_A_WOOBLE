using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class GameManager : MonoBehaviour
{
    // Patrón Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    [Header("Referencias de Interacción")]
    [Tooltip("Arrastra aquí el objeto con el script NotebookController.")]
    public NotebookController notebookController;

    [Tooltip("Arrastra aquí el objeto con el script UIManager.")]
    public UIManager uiManager;

    [Header("VR Sickness Mitigation")]
    [Tooltip("Arrastra el componente TunnelingVignetteController del XR Rig/Camera.")]
    [SerializeField]
    private TunnelingVignetteController _vignetteController;

    [Tooltip("El tiempo que tarda el fundido a negro (Ease In Time) y el fundido a abierto (Ease Out Time).")]
    public float VignetteFadeDuration = 0.5f;

    private const int MainMenuSceneIndex = 0;

    private SceneTransitionProvider _sceneTransitionProvider;
    private VignetteParameters _transitionVignetteParameters;

    // << CLASE SceneTransitionProvider (se mantiene sin cambios) >>
    private class SceneTransitionProvider : ITunnelingVignetteProvider
    {
        public VignetteParameters Parameters;

        public VignetteParameters vignetteParameters => Parameters;

        public SceneTransitionProvider(VignetteParameters p)
        {
            Parameters = p;
        }
    }


    void Awake()
    {
        // Implementación del Singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        // Inicialización del Proveedor
        _transitionVignetteParameters = new VignetteParameters
        {
            apertureSize = 0.0f,
            easeInTime = VignetteFadeDuration,
            easeOutTime = VignetteFadeDuration,
        };
        _sceneTransitionProvider = new SceneTransitionProvider(_transitionVignetteParameters);

        // CORRECCIÓN CS0618: Usar FindAnyObjectByType para buscar el UIManager
        if (uiManager == null)
        {
            // Usamos FindAnyObjectByType<T>() para reemplazar FindObjectOfType<T>()
            uiManager = FindAnyObjectByType<UIManager>(); // <-- ¡CORRECCIÓN AQUÍ!
            if (uiManager == null)
            {
                Debug.LogError("UIManager no encontrado en la escena. Asigna la referencia en el Inspector.");
            }
        }
    }

    // Suscripción a eventos de escena
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Se llama una vez que una escena ha sido cargada. Inicia el fundido de salida de la viñeta.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            _vignetteController.EndTunnelingVignette(_sceneTransitionProvider);
            Debug.Log("GameManager: Viñeta abierta (Fade Out) después de cargar la escena.");
        }
    }

    // --- FUNCIONES LLAMADAS POR EL UIManager O LA LÓGICA DEL JUEGO ---

    /// <summary>
    /// Coroutine que maneja el fundido a negro y la carga de escena de forma segura.
    /// </summary>
    public IEnumerator TransitionToScene(int sceneIndex)
    {
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            // 1. FUNDIDO A NEGRO (Fade-In)
            _vignetteController.BeginTunnelingVignette(_sceneTransitionProvider);

            // Espera a que el fundido a negro termine.
            yield return new WaitForSeconds(VignetteFadeDuration);
        }
        else
        {
            // Fallback si no hay viñeta.
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Carga la escena *después* de que la viñeta esté completamente cerrada.
        SceneManager.LoadScene(sceneIndex);
    }


    /// <summary>
    /// Reinicia el nivel actual (llamado por el UIManager).
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("GameManager: Reiniciando nivel...");
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        StartCoroutine(TransitionToScene(currentSceneIndex));
    }

    /// <summary>
    /// Carga la siguiente escena (llamado por el UIManager).
    /// </summary>
    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("GameManager: Cargando el siguiente nivel (Index: " + nextSceneIndex + ")");
            StartCoroutine(TransitionToScene(nextSceneIndex));
        }
        else
        {
            Debug.LogWarning("GameManager: No hay más niveles. Cargando menú principal.");
            StartCoroutine(TransitionToScene(MainMenuSceneIndex));
        }
    }

    /// <summary>
    /// Sale de la aplicación.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("GameManager: Cerrando la aplicación...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- EVENTOS DE JUEGO ---

    /// <summary>
    /// Función llamada por la lógica del juego al completar los objetivos.
    /// </summary>
    public void OnMissionComplete()
    {
        Debug.Log("GameManager: Misión Wooble completada. Mostrando UI de fin de nivel.");

        if (uiManager != null)
        {
            // Inicia la secuencia de fin de nivel en el UIManager.
            uiManager.StartEndLevelSequence();
        }
        else
        {
            Debug.LogError("GameManager: No se pudo iniciar la secuencia de fin de nivel. UIManager es null.");
        }
    }

    public void NotifyWoobleEscaped()
    {
        // ... (Tu lógica de notificación)
        Debug.Log("ALERTA: Wooble Escapado! (Notificación Simulada)");
    }
}