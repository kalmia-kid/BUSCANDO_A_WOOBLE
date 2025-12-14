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
    // Referencia que NO debe persistir entre escenas.
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
        // 1. Implementación del Singleton (persistencia)
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // 2. Inicialización del Proveedor de Viñeta
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
    /// Se llama una vez que una escena ha sido cargada. Actualiza todas las referencias y realiza el Fade Out.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ********** CORRECCIÓN CRÍTICA: RE-ADQUISICIÓN DE REFERENCIAS DE ESCENA **********
        // Siempre buscamos la nueva instancia ya que el objeto anterior fue destruido.

        // 1. Re-adquirir el UIManager 
        uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null) Debug.Log("GameManager: Nueva instancia de UIManager adquirida.");
        else Debug.LogError("GameManager: UIManager NO ENCONTRADO en la escena " + scene.name);


        // 2. Re-adquirir el NotebookController
        notebookController = FindAnyObjectByType<NotebookController>();
        if (notebookController != null) Debug.Log("GameManager: Nueva instancia de NotebookController adquirida.");


        // ********** LÓGICA DE FADE OUT DE VIÑETA (CORRECCIÓN FINAL) **********

        // La referencia _vignetteController (persistente) se usa como fallback, pero siempre intentamos buscar el objeto en la escena activa.
        TunnelingVignetteController activeVignetteController = FindAnyObjectByType<TunnelingVignetteController>();

        // Asignamos la referencia encontrada (si existe) al campo persistente.
        _vignetteController = activeVignetteController;

        if (activeVignetteController != null && _sceneTransitionProvider != null)
        {
            // EndTunnelingVignette iniciará el fundido de apertura usando el easeOutTime definido (0.5s).
            activeVignetteController.EndTunnelingVignette(_sceneTransitionProvider);

            Debug.Log($"GameManager: Fade Out de Viñeta iniciado en la escena {scene.name}. Duración: {VignetteFadeDuration}s.");
        }
    }

    // --- FUNCIONES LLAMADAS POR EL UIManager O LA LÓGICA DEL JUEGO ---

    /// <summary>
    /// Coroutine que maneja el fundido a negro y la carga de escena de forma segura.
    /// </summary>
    public IEnumerator TransitionToScene(int sceneIndex)
    {
        // 1. FUNDIDO A NEGRO (Fade-In)
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            // Antes de la transición, invalidamos la referencia persistente para forzar la búsqueda en la nueva escena.
            // Aunque EndTunnelingVignette hace esto de forma implicita, esto refuerza la lógica.

            _vignetteController.BeginTunnelingVignette(_sceneTransitionProvider);

            // Espera a que el fundido a negro termine.
            yield return new WaitForSeconds(VignetteFadeDuration);
        }
        else
        {
            // Fallback (tiempo sin fundido)
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Carga la escena (el OnSceneLoaded se encargará del Fade Out)
        SceneManager.LoadScene(sceneIndex);
    }


    public void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Reiniciamos el nivel de forma segura.
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
            Debug.Log("GameManager: Misión completada. Llamando a UIManager.StartEndLevelSequence().");
            uiManager.StartEndLevelSequence();
        }
        else
        {
            // Ahora este error debería ser raro si el UIManager está presente en la escena.
            Debug.LogError("GameManager: uiManager es NULL. Verifica que el UIManager esté presente en la escena y en la capa correcta.");
        }
    }

    public void NotifyWoobleEscaped()
    {
        Debug.Log("ALERTA: Wooble Escapado! (Notificación Simulada)");
    }
}