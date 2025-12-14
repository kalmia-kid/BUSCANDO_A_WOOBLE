using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------------------
    // AÑADIDO: Implementación de Singleton para fácil acceso
    // -------------------------------------------------------------------
    public static UIManager Instance { get; private set; }

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

    // --- Referencias Públicas (Asignar en el Inspector de Unity) ---

    [Header("Componentes")]
    [Tooltip("Referencia al script que controla la animación del objeto 3D.")]
    public NotebookController NotebookController; // AÑADIDO

    [Header("Paneles de UI")]
    public GameObject MissionStartUIPanel; // El panel de la misión/portafolio
    public GameObject EndLevelUIPanel;     // El panel de fin de nivel

    [Header("Botones")]
    public Button Button_NextLevel;        // El botón para avanzar de nivel

    // AÑADIDO: Botón para iniciar la misión y cerrar el portafolio
    public Button Button_StartMission;

    // ------------------------------------------------------------------

    void Start()
    {
        // 1. Inicialización: Aseguramos el estado inicial de los paneles
        EndLevelUIPanel.SetActive(false);

        // El MissionStartUIPanel debe estar en el estado que se desea al inicio del nivel (ej: activo para que se vea el inicio).
        // MissionStartUIPanel.SetActive(true); 

        // 2. Asignación de Listeners
        if (Button_NextLevel != null)
        {
            Button_NextLevel.onClick.AddListener(CargarSiguienteNivel);
        }
        if (Button_StartMission != null) // AÑADIDO: Lógica para el botón de iniciar misión
        {
            Button_StartMission.onClick.AddListener(HideMissionUIAndNotebook);
        }

        // 3. Verificación del NotebookController
        if (NotebookController == null)
        {
            Debug.LogError("ERROR: Asigna el NotebookController en el UIManager.");
        }
    }

    // =================================================================
    // >>> LÓGICA DE INICIO Y CIERRE DE MISIÓN (NUEVAS FUNCIONES) <<<
    // =================================================================

    /// <summary>
    /// Llamado desde el UIManager.Start() o desde el GameManager al inicio.
    /// Despliega el cuaderno y muestra el panel de inicio.
    /// </summary>
    public void ShowMissionUIAndDeployNotebook()
    {
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(true); // Mostrar el panel de misión
        }

        // Pide al NotebookController que haga la animación de apertura
        if (NotebookController != null)
        {
            NotebookController.DeployAndShowMissionUI();
        }
    }

    /// <summary>
    /// Llamado por el Button_StartMission. Oculta el panel de UI y el objeto 3D.
    /// </summary>
    public void HideMissionUIAndNotebook()
    {
        // 1. Ocultar el panel de UI inmediatamente
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(false);
        }

        // 2. Pide al NotebookController que haga la animación de cierre
        if (NotebookController != null)
        {
            NotebookController.HideNotebook();
        }

        // Opcional: GameManager.Instance.ResumeGame(); o similar
    }

    // =================================================================
    // >>> LÓGICA DE FIN DE NIVEL (MODIFICADA) <<<
    // =================================================================

    /// <summary>
    /// Se llama desde el GameManager al finalizar el nivel. 
    /// Delega la animación al NotebookController.
    /// </summary>
    public void StartEndLevelSequence()
    {
        if (NotebookController != null)
        {
            // El NotebookController se encargará de hacer la animación de apertura
            // y luego llamará a ActivateEndLevelUIPanel()
            NotebookController.DeployAndShowEndLevelUI();
        }
        else
        {
            Debug.LogError("ERROR: NotebookController no está asignado.");
        }
    }

    /// <summary>
    /// Llamado por el NotebookController después de que la animación de apertura ha terminado.
    /// Cumple el requisito: MissionStartUIPanel se oculta cuando aparece EndLevelUIPanel.
    /// </summary>
    public void ActivateEndLevelUIPanel() // NUEVA FUNCIÓN PÚBLICA DE CALLBACK
    {
        // 1. Ocultar el panel de misión/portafolio
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(false);
        }

        // 2. Mostrar el panel de fin de nivel
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(true);
        }
    }

    // =================================================================
    // >>> FUNCIÓN DE BOTÓN (SIN CAMBIOS) <<<
    // =================================================================

    /// <summary>
    /// Se llama automáticamente al presionar el Button_NextLevel.
    /// Cumple el requisito: EndLevelUIPanel solo desaparece al presionar el botón.
    /// </summary>
    private void CargarSiguienteNivel()
    {
        if (EndLevelUIPanel != null)
        {
            // 1. Ocultar el panel de fin de nivel
            EndLevelUIPanel.SetActive(false);
        }

        // 2. Lógica para avanzar de escena
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("¡Fin del juego! No hay más escenas en Build Settings.");
            // Opcional: Volver al menú principal, etc.
        }
    }
}