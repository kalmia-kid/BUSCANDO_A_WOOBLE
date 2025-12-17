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
    [SerializeField]
    private TunnelingVignetteController _vignetteController;

    public float VignetteFadeDuration = 0.5f;

    // --- NUEVA VARIABLE DE CONTROL ---
    // Se inicia en true. Una vez que cargue la primera escena, pasará a false para siempre.
    private bool _isFirstLoad = true;
    // -------------------------------

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
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        _transitionVignetteParameters = new VignetteParameters
        {
            apertureSize = 0.0f,
            easeInTime = VignetteFadeDuration,
            easeOutTime = VignetteFadeDuration,
            vignetteColor = Color.black
        };
        _sceneTransitionProvider = new SceneTransitionProvider(_transitionVignetteParameters);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Re-adquirir referencias
        uiManager = FindAnyObjectByType<UIManager>();
        notebookController = FindAnyObjectByType<NotebookController>();

        // 2. Buscar el controlador de viñeta de la nueva escena
        TunnelingVignetteController activeVignetteController = FindAnyObjectByType<TunnelingVignetteController>();
        _vignetteController = activeVignetteController;

        // ********** LÓGICA DE PRIMERA CARGA VS TRANSICIONES **********

        if (_isFirstLoad)
        {
            // ESCENARIO A: Es la primera vez que abres el juego (o das Play en Unity).
            Debug.Log("GameManager: Primera carga detectada. NO se aplica efecto de entrada.");

            // Marcamos que ya no es la primera vez.
            _isFirstLoad = false;

            // Opcional: Asegurarnos de que la viñeta esté abierta/desactivada por si acaso
            // if (activeVignetteController != null) activeVignetteController.gameObject.SetActive(false);
        }
        else
        {
            // ESCENARIO B: Es un reinicio de nivel o un cambio de nivel.
            // Aquí SI queremos el efecto de "Pantalla Negra -> Transparente".
            if (activeVignetteController != null && _sceneTransitionProvider != null)
            {
                StartCoroutine(ForceBlackThenFadeIn(activeVignetteController));
            }
        }
    }

    /// <summary>
    /// Pone la pantalla en negro INSTANTÁNEAMENTE y luego hace el Fade Out.
    /// </summary>
    private IEnumerator ForceBlackThenFadeIn(TunnelingVignetteController controller)
    {
        float originalEaseIn = _transitionVignetteParameters.easeInTime;
        _transitionVignetteParameters.easeInTime = 0f; // Tiempo 0 = Instantáneo

        controller.BeginTunnelingVignette(_sceneTransitionProvider); // Cierra a negro de golpe

        yield return null; // Espera un frame para que se renderice el negro

        _transitionVignetteParameters.easeInTime = originalEaseIn; // Restaura tiempo suave
        controller.EndTunnelingVignette(_sceneTransitionProvider); // Abre suavemente

        Debug.Log($"GameManager: Fade In (Transición) completado.");
    }

    // --- EL RESTO DEL CÓDIGO SE MANTIENE IGUAL ---

    public IEnumerator TransitionToScene(int sceneIndex)
    {
        if (_vignetteController != null && _sceneTransitionProvider != null)
        {
            _transitionVignetteParameters.easeInTime = VignetteFadeDuration;
            _vignetteController.BeginTunnelingVignette(_sceneTransitionProvider);
            yield return new WaitForSeconds(VignetteFadeDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
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

    public void OnMissionComplete()
    {
        if (uiManager != null)
        {
            uiManager.StartEndLevelSequence();
        }
    }

    public void NotifyWoobleEscaped()
    {
        Debug.Log("ALERTA: Wooble Escapado!");
    }
}