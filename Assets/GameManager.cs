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
    /// Reinicia el nivel actual.
    /// Esta función se conecta al botón "Reiniciar" del cuaderno.
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("GameManager: Reiniciando nivel...");

        // Opcional: Esconder el cuaderno si todavía está activo
        if (notebookController != null && notebookController.IsActive())
        {
            notebookController.ToggleNotebook();
        }

        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Carga la siguiente escena en el orden de Build Settings.
    /// Esta función se conecta al botón "Siguiente Nivel" del cuaderno.
    /// </summary>
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("GameManager: Cargando el siguiente nivel (Index: " + nextSceneIndex + ")");
            // Opcional: Esconder el cuaderno si todavía está activo
            if (notebookController != null && notebookController.IsActive())
            {
                // ToggleNotebook() maneja la animación de guardado.
                notebookController.ToggleNotebook();
            }
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("GameManager: No hay más niveles. Cargando menú principal o escena final.");
            // Si no hay más niveles, vuelve al menú principal.
            SceneManager.LoadScene(MainMenuSceneIndex);
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
    /// Función de ejemplo que se llamaría cuando Wooble es atrapado y la misión se completa.
    /// </summary>
    public void OnMissionComplete()
    {
        Debug.Log("GameManager: Misión Wooble completada. Mostrando UI de fin de nivel.");

        // 1. Mostrar el cuaderno
        if (notebookController != null)
        {
            notebookController.ShowEndLevelUI();
        }

        // 2. Aquí iría la lógica para detener el tiempo, pausar el input principal, etc.
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