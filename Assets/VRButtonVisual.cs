using UnityEngine;

public class VRButtonVisual : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Arrastra aquí el objeto cilindro que debe bajar.")]
    public Transform movingPart; 
    
    [Tooltip("Distancia que baja en el eje Y (ej: 0.05).")]
    public float pressDepth = 0.02f;
    
    [Tooltip("Velocidad de la animación.")]
    public float returnSpeed = 10f;

    // Variables privadas para recordar posiciones
    private Vector3 initialPos;
    private Vector3 pressedPos;
    private bool isDown = false;

    void Start()
    {
        // 1. AUTO-REPARACIÓN: Si se te olvidó asignar el objeto, usamos el propio transform
        if (movingPart == null) 
        {
            movingPart = transform;
            Debug.LogWarning($"⚠️ [VRButtonVisual] No asignaste 'Moving Part' en {gameObject.name}. Usando el propio objeto.");
        }

        // 2. CÁLCULOS: Guardamos dónde empieza y dónde acaba
        initialPos = movingPart.localPosition;
        // Asumimos que el botón baja en su eje Y negativo local
        pressedPos = initialPos - new Vector3(0, pressDepth, 0);
    }

    void Update()
    {
        // 3. ANIMACIÓN: Mueve el objeto suavemente hacia el objetivo
        Vector3 target = isDown ? pressedPos : initialPos;
        
        // Solo movemos si hay una diferencia notable (ahorra recursos)
        if (Vector3.Distance(movingPart.localPosition, target) > 0.001f)
        {
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, target, Time.deltaTime * returnSpeed);
        }
    }

    // --- MÉTODOS PÚBLICOS (Conecta esto al XR Simple Interactable) ---

    [ContextMenu("TEST: Forzar Bajar")] // Truco: Te permite probarlo desde el inspector
    public void OnDown()
    {
        Debug.Log($"⬇️ [TEST] '{gameObject.name}' -> OnDown() EJECUTADO. El botón debería bajar.");
        isDown = true;
    }

    [ContextMenu("TEST: Forzar Subir")]
    public void OnUp()
    {
        Debug.Log($"⬆️ [TEST] '{gameObject.name}' -> OnUp() EJECUTADO. El botón debería subir.");
        isDown = false;
    }
}