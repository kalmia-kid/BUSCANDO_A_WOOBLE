using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
// << AÑADIDO: NECESARIO para acceder a VignetteParameters y ITunnelingVignetteProvider >>
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

public class GameManager : MonoBehaviour
{
    // Patrón Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    [Header("Referencias de Interacción")]
    [Tooltip("Arrastra aquí el objeto con el script NotebookController.")]
    public NotebookController notebookController;

    [Header("VR Sickness Mitigation")]
    [Tooltip("Arrastra el componente TunnelingVignetteController del XR Rig/Camera.")]
    [SerializeField]
    private TunnelingVignetteController _vignetteController;

    // << NUEVO CAMPO: TIEMPO DE FUNDIDO MANUAL >>
    [Tooltip("El tiempo que tarda el fundido a negro (Ease In Time) y el fundido a abierto (Ease Out Time).")]
    public float VignetteFadeDuration = 0.5f;

    private const int MainMenuSceneIndex = 0;

    // << NUEVOS CAMPOS: Para implementar el Provider Pattern >>
    private SceneTransitionProvider _sceneTransitionProvider;
    private VignetteParameters _transitionVignetteParameters;


    // << NUEVA CLASE: Implementa la interfaz ITunnelingVignetteProvider >>
    // Esta clase nos permite llamar a Begin/EndTunnelingVignette
    private class SceneTransitionProvider : ITunnelingVignetteProvider
    {
        public VignetteParameters Parameters;

        // Propiedad requerida por la interfaz, que devuelve nuestros parámetros
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

        // << CAMBIO CRÍTICO: Inicialización del Proveedor >>
        // Creamos los parámetros para el fundido a negro total (ApertureSize = 0.0f)
        _transitionVignetteParameters = new VignetteParameters
        {
            apertureSize = 0.0f,
            easeInTime = VignetteFadeDuration,
            easeOutTime = VignetteFadeDuration,
        };
        _sceneTransitionProvider = new SceneTransitionProvider(_transitionVignetteParameters);
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
            // << CAMBIO CRÍTICO: Usamos EndTunnelingVignette para iniciar el fundido de salida >>
            // Al llamar End, el controlador sabe que el efecto ya no es necesario e inicia el Ease Out.
            _vignetteController.EndTunnelingVignette(_sceneTransitionProvider);
            Debug.Log("GameManager: Viñeta abierta (Fade Out) después de cargar la escena.");
        }
    }

    // --- FUNCIONES LLAMADAS POR LOS BOTONES DEL CUADERNO ---

    private void HideNotebookAndLoadScene(int sceneIndex)
    {
        // 1. Esconde la UI del cuaderno si está activa.
        if (notebookController != null && notebookController.IsActive())
        {
            notebookController.ToggleNotebook();
        }

        // 2. Inicia la transición segura.
        StartCoroutine(TransitionToScene(sceneIndex));
    }

    /// <summary>
    /// Coroutine que maneja el fundido a negro y la carga de escena.
    /// </summary>
    private IEnumerator TransitionToScene(int sceneIndex)
    {
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            // 1. FUNDIDO A NEGRO (Fade-In): Inicia el efecto con nuestro proveedor.
            // El controlador usa el 'easeInTime' de nuestros _transitionVignetteParameters (VignetteFadeDuration).
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
    /// Reinicia el nivel actual.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("GameManager: Reiniciando nivel...");
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        HideNotebookAndLoadScene(currentSceneIndex);
    }

    /// <summary>
    /// Carga la siguiente escena.
    /// </summary>
    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("GameManager: Cargando el siguiente nivel (Index: " + nextSceneIndex + ")");
            HideNotebookAndLoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("GameManager: No hay más niveles. Cargando menú principal.");
            HideNotebookAndLoadScene(MainMenuSceneIndex);
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

    // --- FUNCIONES LLAMADAS POR LA LÓGICA DEL JUEGO ---

    public void OnMissionComplete()
    {
        Debug.Log("GameManager: Misión Wooble completada. Mostrando UI de fin de nivel.");
        if (notebookController != null)
        {
            notebookController.ShowEndLevelUI();
        }
    }

    public void NotifyWoobleEscaped()
    {
        // Nota: Asumiendo que WatchNotifier.Instance tiene una instancia válida
        if (WatchNotifier.Instance != null)
        {
            WatchNotifier.Instance.DisplayAlarm("ALERTA: Wooble Escapado!");
        }
    }
}