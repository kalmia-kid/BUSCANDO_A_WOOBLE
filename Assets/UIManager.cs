using UnityEngine;
using UnityEngine.UI;
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
            // Destruye instancias duplicadas que se crean en la nueva escena.
            Destroy(gameObject);
        }
    }

    // --- Referencias Públicas ---

    [Header("Componentes")]
    [Tooltip("Referencia al script que controla la animación del objeto 3D (cuaderno).")]
    public NotebookController NotebookController;

    [Header("Paneles de UI")]
    public GameObject MissionStartUIPanel;
    public GameObject EndLevelUIPanel;

    [Header("Botones")]
    public Button Button_NextLevel;

    [Tooltip("Botón de Reinicio en el Panel de Fin de Nivel.")]
    public Button Button_RestartLevel_End;

    [Tooltip("Botón de Reinicio en el Panel de Inicio de Misión (o Pausa).")]
    public Button Button_RestartLevel_Start;

    // ------------------------------------------------------------------

    void Start()
    {
        // 1. Inicialización
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(false);
        }

        // 2. Asignación de Listeners (CRÍTICO: Limpiar para asegurar que funciona)

        // Botón Siguiente Nivel
        if (Button_NextLevel != null)
        {
            Button_NextLevel.onClick.RemoveAllListeners();
            Button_NextLevel.onClick.AddListener(OnNextLevelButtonPressed);
        }

        // Botón Reinicio (Panel de Fin de Nivel)
        if (Button_RestartLevel_End != null)
        {
            Button_RestartLevel_End.onClick.RemoveAllListeners();
            Button_RestartLevel_End.onClick.AddListener(OnRestartLevelButtonPressed);
        }

        // Botón Reinicio (Panel de Inicio/Pausa)
        if (Button_RestartLevel_Start != null)
        {
            Button_RestartLevel_Start.onClick.RemoveAllListeners();
            Button_RestartLevel_Start.onClick.AddListener(OnRestartLevelButtonPressed);
        }

        // 3. Verificación
        if (NotebookController == null)
        {
            Debug.LogError("ERROR: Asigna el NotebookController en el UIManager.");
        }
    }

    // =================================================================
    // >>> LÓGICA DE INICIO Y FIN DE MISIÓN <<<
    // =================================================================

    /// <summary>
    /// Despliega el cuaderno y muestra el panel de inicio (Llamado por GameManager).
    /// </summary>
    public void ShowMissionUIAndDeployNotebook()
    {
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(true);
        }

        if (NotebookController != null)
        {
            NotebookController.DeployAndShowMissionUI();
        }
    }

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
            ActivateEndLevelUIPanel();
        }
    }

    /// <summary>
    /// Llamado por el NotebookController después de que la animación de apertura ha terminado.
    /// **(Esta es la función que NotebookController necesita)**
    /// </summary>
    public void ActivateEndLevelUIPanel()
    {
        // 1. Ocultar el panel de misión (si estuviera activo)
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
    // >>> FUNCIONES DE BOTÓN <<<
    // =================================================================

    /// <summary>
    /// Se llama al presionar el Button_NextLevel. Delega la transición al GameManager.
    /// </summary>
    private void OnNextLevelButtonPressed()
    {
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("UIManager: Botón Siguiente Nivel presionado. Iniciando GameManager.NextLevel().");
            GameManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogError("UIManager: No se encontró una instancia de GameManager.");
        }
    }

    /// <summary>
    /// Delega el reinicio al GameManager. Esta función es llamada por AMBOS botones de reinicio.
    /// </summary>
    private void OnRestartLevelButtonPressed()
    {
        // Ocultamos AMBOS paneles antes de la transición para evitar errores visuales
        if (EndLevelUIPanel != null)
        {
            EndLevelUIPanel.SetActive(false);
        }
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("UIManager: Botón Reiniciar Nivel presionado. Iniciando GameManager.RestartLevel().");
            GameManager.Instance.RestartLevel();
        }
        else
        {
            Debug.LogError("UIManager: No se encontró una instancia de GameManager. El reinicio falló.");
        }
    }
}