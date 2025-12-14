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

    // ¡NUEVAS REFERENCIAS AÑADIDAS!
    [Tooltip("Botón para Salir del Juego en la UI de Fin de Nivel.")]
    public Button Button_QuitGame_End;

    [Tooltip("Botón para Salir del Juego en la UI de Inicio/Pausa de Misión.")]
    public Button Button_QuitGame_Start;

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

        // Botones de Reinicio
        if (Button_RestartLevel_End != null)
        {
            Button_RestartLevel_End.onClick.RemoveAllListeners();
            Button_RestartLevel_End.onClick.AddListener(OnRestartLevelButtonPressed);
        }

        if (Button_RestartLevel_Start != null)
        {
            Button_RestartLevel_Start.onClick.RemoveAllListeners();
            Button_RestartLevel_Start.onClick.AddListener(OnRestartLevelButtonPressed);
        }

        // ¡LISTENERS AÑADIDOS! Botones para Salir del Juego
        if (Button_QuitGame_End != null)
        {
            Button_QuitGame_End.onClick.RemoveAllListeners();
            Button_QuitGame_End.onClick.AddListener(OnQuitGameButtonPressed); // Llama a la misma función
            Debug.Log("UIManager: Listener de Salir (Fin de Nivel) reasignado.");
        }

        if (Button_QuitGame_Start != null)
        {
            Button_QuitGame_Start.onClick.RemoveAllListeners();
            Button_QuitGame_Start.onClick.AddListener(OnQuitGameButtonPressed); // Llama a la misma función
            Debug.Log("UIManager: Listener de Salir (Inicio/Pausa) reasignado.");
        }

        // 3. Verificación
        if (NotebookController == null)
        {
            Debug.LogError("ERROR: Asigna el NotebookController en el UIManager.");
        }
    }

    // =================================================================
    // >>> LÓGICA DE UI Y ANIMACIÓN <<<
    // =================================================================

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
    /// </summary>
    public void ActivateEndLevelUIPanel()
    {
        if (MissionStartUIPanel != null)
        {
            MissionStartUIPanel.SetActive(false);
        }

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

    private void OnRestartLevelButtonPressed()
    {
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

    /// <summary>
    /// Delega la acción de salir al GameManager. Llamada por AMBOS botones de salir.
    /// </summary>
    private void OnQuitGameButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("UIManager: Botón Salir presionado. Llamando a GameManager.QuitGame().");
            GameManager.Instance.QuitGame();
        }
        else
        {
            Debug.LogError("UIManager: No se encontró una instancia de GameManager para salir del juego.");
        }
    }
}