using UnityEngine;

public class Wooble : MonoBehaviour
{
    // Asegúrate de asignar este Rigidbody en el Inspector de Unity
    [Tooltip("Referencia al Rigidbody de Wooble.")]
    public Rigidbody woobleRigidbody;

    // Opcional: Si Wooble usa un script personalizado para moverse, añádelo aquí
    // public WoobleMovement WoobleMovementScript;

    public bool IsCaptured { get; private set; } = false;

    void Awake()
    {
        // Intenta obtener el Rigidbody automáticamente si no se ha asignado en el Inspector
        if (woobleRigidbody == null)
        {
            woobleRigidbody = GetComponent<Rigidbody>();
        }

        // Opcional: Intenta obtener el script de movimiento
        // if (WoobleMovementScript == null)
        // {
        //     TryGetComponent(out WoobleMovementScript);
        // }
    }


    /// <summary>
    /// Inicia el proceso de captura, bloqueando el movimiento y estableciendo el estado.
    /// </summary>
    public void StartCapture()
    {
        if (IsCaptured) return;

        // Bloquea cualquier script de movimiento aquí (si tienes uno)
        // if (WoobleMovementScript != null)
        // {
        //     WoobleMovementScript.enabled = false;
        // }

        IsCaptured = true;
        Debug.Log("Wooble: Enganche inicializado.");
    }

    /// <summary>
    /// Desactiva la simulación física y el movimiento para permitir que el dispositivo lo ancle.
    /// ¡ESTA ES LA FUNCIÓN CLAVE PARA LA CORRECCIÓN!
    /// </summary>
    public void DisablePhysicsAndMovement()
    {
        if (woobleRigidbody != null)
        {
            // Detener el movimiento actual
            woobleRigidbody.linearVelocity = Vector3.zero;
            woobleRigidbody.angularVelocity = Vector3.zero;

            // Hacer que sea cinemático: su movimiento será controlado únicamente por su transform
            // (y, por extensión, por su nuevo padre, el dispositivo de captura).
            woobleRigidbody.isKinematic = true;

            // Desactivar la detección de colisiones si es necesario.
            // woobleRigidbody.detectCollisions = false; 
        }
        else
        {
            Debug.LogWarning("Wooble: Rigidbody es NULL. La física no se pudo detener. Verifica la asignación en el Inspector.");
        }

        // NOTA: Si usas CharacterController o NavMeshAgent, debes deshabilitarlos aquí.
    }


    /// <summary>
    /// Desactiva el GameObject de Wooble. Llamado por CaptureDevice.
    /// </summary>
    public void FinalizeDisappearance()
    {
        gameObject.SetActive(false);
    }
}