using UnityEngine;
using UnityEngine.InputSystem;

public class NotebookInputListener : MonoBehaviour
{
    [Tooltip("La Input Action que queremos escuchar (ej. LeftHand/ToggleNotebook).")]
    public InputActionProperty notebookToggleAction;

    [Tooltip("Referencia al controlador del cuaderno.")]
    public NotebookController notebookController;

    void OnEnable()
    {
        // Suscribirse al evento 'performed' de la Input Action
        notebookToggleAction.action.performed += OnNotebookAction;
        notebookToggleAction.action.Enable();
    }

    void OnDisable()
    {
        // Desuscribirse al deshabilitarse el objeto
        notebookToggleAction.action.performed -= OnNotebookAction;
        notebookToggleAction.action.Disable();
    }

    private void OnNotebookAction(InputAction.CallbackContext context)
    {
        // Llamar a la función del cuaderno cuando el botón sea presionado
        if (notebookController != null)
        {
            notebookController.ToggleNotebook();
        }
        else
        {
            Debug.LogError("NotebookController no está asignado en el NotebookInputListener.");
        }
    }
}