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
    public NotebookController notebookController;

    public UIManager uiManager;

    [Header("VR Sickness Mitigation")]
    // [SerializeField] se mantiene para la asignación en el inspector, pero la re-adquirimos en runtime.
    [SerializeField]
    private TunnelingVignetteController _vignetteController;

    public float VignetteFadeDuration = 0.5f;

    private const int MainMenuSceneIndex = 0;

    private SceneTransitionProvider _sceneTransitionProvider;
    private VignetteParameters _transitionVignetteParameters;

    // << CLASE SceneTransitionProvider >>
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
            // AÑADIDO: Asegura que el GameManager persiste entre escenas
            DontDestroyOnLoad(this.gameObject);
        }

        // Inicialización del Proveedor (se mantiene igual)
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
            uiManager = FindAnyObjectByType<UIManager>();
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
        // ********** CORRECCIÓN DEL FADE OUT **********
        TunnelingVignetteController activeVignetteController = _vignetteController;

        // 1. Si la referencia original es nula (o apunta a un objeto destruido), busca el nuevo controlador en la escena.
        if (activeVignetteController == null)
        {
            activeVignetteController = FindAnyObjectByType<TunnelingVignetteController>();
        }

        if (activeVignetteController != null && _sceneTransitionProvider != null)
        {
            // 2. Ejecuta el fundido de salida en el controlador activo de la nueva escena.
            activeVignetteController.EndTunnelingVignette(_sceneTransitionProvider);

            // Opcional: Actualizar la referencia del GameManager al controlador de la nueva escena
            _vignetteController = activeVignetteController;

            Debug.Log("GameManager: Viñeta abierta (Fade Out) después de cargar la escena.");
        }
        // **********************************************
    }

    // --- FUNCIONES LLAMADAS POR EL UIManager O LA LÓGICA DEL JUEGO ---

    /// <summary>
    /// Coroutine que maneja el fundido a negro y la carga de escena de forma segura.
    /// </summary>
    public IEnumerator TransitionToScene(int sceneIndex)
    {
        // El BeginTunnelingVignette siempre usa la referencia que tiene el GameManager
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            // 1. FUNDIDO A NEGRO (Fade-In)
            _vignetteController.BeginTunnelingVignette(_sceneTransitionProvider);

            // Espera a que el fundido a negro termine.
            yield return new WaitForSeconds(VignetteFadeDuration);
        }
        else
        {
            // Fallback
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Carga la escena (el OnSceneLoaded se encargará del Fade Out)
        SceneManager.LoadScene(sceneIndex);
    }


    public void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(TransitionToScene(currentSceneIndex));
    }

    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(TransitionToScene(nextSceneIndex));
        }
        else
        {
            StartCoroutine(TransitionToScene(MainMenuSceneIndex));
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- EVENTOS DE JUEGO ---

    public void OnMissionComplete()
    {
        if (uiManager != null)
        {
            uiManager.StartEndLevelSequence();
        }
    }

    public void NotifyWoobleEscaped()
    {
        Debug.Log("ALERTA: Wooble Escapado! (Notificación Simulada)");
    }
}