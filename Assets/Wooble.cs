using UnityEngine;

public class Wooble : MonoBehaviour
{
    // ... otras propiedades

    public bool IsCaptured { get; private set; } = false;

    /// <summary>
    /// Inicia el proceso de captura, bloqueando el movimiento y estableciendo el estado.
    /// </summary>
    public void StartCapture()
    {
        if (IsCaptured) return;

        // Bloquea cualquier script de movimiento aquí
        // Ejemplo: if (TryGetComponent<WoobleMovement>(out var wm)) wm.enabled = false;

        IsCaptured = true;

        // El dispositivo se encargará de SetActive(false).
        Debug.Log("Wooble: Enganche inicializado.");
    }

    /// <summary>
    /// Desactiva el GameObject de Wooble. Llamado por CaptureDevice.
    /// </summary>
    public void FinalizeDisappearance()
    {
        gameObject.SetActive(false);
    }
}