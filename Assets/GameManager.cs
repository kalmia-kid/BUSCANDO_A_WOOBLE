using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using System.Collections; // Necesario para Corutinas (aunque no se usan directamente aquí, es buena práctica)

public class GameManager : MonoBehaviour
{
    // Patrón Singleton para acceder al GameManager fácilmente desde cualquier otro script (ej. GameManager.Instance.RestartLevel())
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    [Header("Referencias de Interacción")]
    [Tooltip("Arrastra aquí el objeto con el script NotebookController.")]
    public NotebookController notebookController;

    // Índice de la escena del menú principal (0 es el índice común para el menú principal)
    private const int MainMenuSceneIndex = 0;

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
            // Opcional: Si quieres que el GameManager persista entre escenas: DontDestroyOnLoad(this.gameObject);
        }
    }

    // --- FUNCIONES LLAMADAS POR LOS BOTONES DEL CUADERNO ---

    /// <summary>
    /// Intenta esconder la UI del cuaderno y carga una escena.
    /// Esto ayuda a limpiar la UI antes de la transición de escena.
    /// </summary>
    private void HideNotebookAndLoadScene(int sceneIndex)
    {
        // Esconde la UI del cuaderno (si está activo, esto inicia la animación de guardado)
        if (notebookController != null && notebookController.IsActive())
        {
            notebookController.ToggleNotebook();
        }

        // Carga la escena inmediatamente
        SceneManager.LoadScene(sceneIndex);
    }


    /// <summary>
    /// Reinicia el nivel actual.
    /// Esta función se conecta al botón "Reiniciar" del cuaderno.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("GameManager: Reiniciando nivel...");

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        HideNotebookAndLoadScene(currentSceneIndex);
    }

    /// <summary>
    /// Carga la siguiente escena en el orden de Build Settings.
    /// Esta función se conecta al botón "Siguiente Nivel" del cuaderno.
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
            // Si no hay más niveles, vuelve al menú principal.
            HideNotebookAndLoadScene(MainMenuSceneIndex);
        }
    }

    /// <summary>
    /// Sale de la aplicación.
    /// Esta función se conecta al botón "Salir" del cuaderno.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("GameManager: Cerrando la aplicación...");
        Application.Quit();

        // Nota: Application.Quit() solo funciona en builds. 
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- FUNCIONES LLAMADAS POR LA LÓGICA DEL JUEGO ---

    /// <summary>
    /// Función que se llama cuando Wooble es atrapado y la misión se completa.
    /// Inicia la secuencia de Fin de Nivel en la UI del cuaderno.
    /// </summary>
    public void OnMissionComplete()
    {
        Debug.Log("GameManager: Misión Wooble completada. Mostrando UI de fin de nivel.");

        // 1. Mostrar el cuaderno (el cuaderno manejará la lógica de abrirse y mostrar la UI después de la animación)
        if (notebookController != null)
        {
            notebookController.ShowEndLevelUI();
        }

        // 2. Aquí iría la lógica para detener el tiempo, pausar el input principal, etc.
        // Time.timeScale = 0f; // Si deseas pausar completamente el juego.
    }

    // Puedes añadir una función para la alarma del reloj aquí:
    public void NotifyWoobleEscaped()
    {
        // Asegúrate de que la instancia del notificador exista antes de llamar
        if (WatchNotifier.Instance != null)
        {
            WatchNotifier.Instance.DisplayAlarm("ALERTA: Wooble Escapado!");
        }
    }
}