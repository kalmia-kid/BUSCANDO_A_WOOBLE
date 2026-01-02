using UnityEngine;
using System.Collections;

public class SlidingDoor : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Hacia dónde se mueve la puerta (en metros). Ej: (2, 0, 0) mueve 2 metros a la derecha.")]
    public Vector3 slideOffset = new Vector3(2f, 0f, 0f);
    
    [Tooltip("Tiempo que tarda en abrirse.")]
    public float duration = 1.0f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private Coroutine movementCoroutine;

    void Start()
    {
        // Guardamos la posición inicial como "Cerrada"
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slideOffset;
    }

    /// <summary>
    /// Función pública para llamar desde el botón
    /// </summary>
    public void ToggleDoor()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        
        isOpen = !isOpen;
        
        // Si isOpen es true, vamos a openPosition, si no a closedPosition
        Vector3 target = isOpen ? openPosition : closedPosition;
        movementCoroutine = StartCoroutine(MoveDoor(target));
    }

    private IEnumerator MoveDoor(Vector3 targetPos)
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;
    }
}