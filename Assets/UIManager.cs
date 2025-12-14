using UnityEngine;
using UnityEngine.UI;
// NOTA: Se necesita SceneManagement para GetActiveScene().buildIndex
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // -------------------------------------------------------------------
    // Implementación de Singleton
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
    public NotebookController NotebookController;

    [Header("Paneles de UI")]
    public GameObject MissionStartUIPanel;
    public GameObject EndLevelUIPanel;

    [Header("Botones")]
    public Button Button_NextLevel;
    public Button Button_StartMission;
    // Si tienes un botón de reinicio en el panel de fin de nivel, añádelo aquí.
    // public Button Button_RestartLevel; 

    // ------------------------------------------------------------------

    void Start()
    {
        // 1. Inicialización: Aseguramos el estado inicial de los paneles
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(false);
        }

        // 2. Asignación de Listeners
        if (Button_NextLevel != null)
        {
            // CORRECCIÓN: Usamos el nuevo nombre de función y delegamos a GameManager
            Button_NextLevel.onClick.AddListener(OnNextLevelButtonPressed);
        }
        if (Button_StartMission != null)
        {
            // CORRECCIÓN: Attach listener a la función pública definida abajo.
            Button_StartMission.onClick.AddListener(HideMissionUIAndNotebook);
        }

        // 3. Verificación del NotebookController
        if (NotebookController == null)
        {
            Debug.LogError("ERROR: Asigna el NotebookController en el UIManager.");
        }
    }

    // =================================================================
    // >>> LÓGICA DE INICIO Y CIERRE DE MISIÓN <<<
    // =================================================================

    // Función que faltaba o estaba mal definida.
    /// <summary>
    /// Llamado por el Button_StartMission. Oculta el panel de UI y el objeto 3D.
    /// </summary>
    public void HideMissionUIAndNotebook() // <--- ¡DEBE SER PÚBLICA!
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
    }

    // ... (ShowMissionUIAndDeployNotebook, si la usas, también debe estar aquí) ...

    // =================================================================
    // >>> LÓGICA DE FIN DE NIVEL <<<
    // =================================================================

    /// <summary>
    /// Se llama desde el GameManager al finalizar el nivel. 
    /// Delega la animación al NotebookController.
    /// </summary>
    public void StartEndLevelSequence()
    {
        if (NotebookController != null)
        {
            NotebookController.DeployAndShowEndLevelUI();
        }
        else
        {
            Debug.LogError("ERROR: NotebookController no está asignado. Activando UI directamente.");
            // Fallback
            ActivateEndLevelUIPanel();
        }
    }

    /// <summary>
    /// Llamado por el NotebookController después de que la animación de apertura ha terminado.
    /// </summary>
    public void ActivateEndLevelUIPanel()
    {
        // 1. Ocultar el panel de misión/portafolio (si estuviera activo)
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(false);
        }

        // 2. Mostrar el panel de fin de nivel
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UIManager: EndLevelUIPanel es nulo. Asigna la referencia.");
        }
    }

    // =================================================================
    // >>> FUNCIÓN DE BOTÓN CORREGIDA PARA USAR EL GAMEMANAGER (CON VIÑETA) <<<
    // =================================================================

    /// <summary>
    /// Se llama al presionar el Button_NextLevel. Oculta la UI y llama al GameManager para la transición segura.
    /// </summary>
    private void OnNextLevelButtonPressed()
    {
        if (EndLevelUIPanel != null)
        {
            // 1. Ocultar el panel de fin de nivel
            EndLevelUIPanel.SetActive(false);
        }

        // 2. Delegar la transición segura (con viñeta) al GameManager.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogError("UIManager: No se encontró una instancia de GameManager. La transición segura de escena falló.");
        }
    }
}