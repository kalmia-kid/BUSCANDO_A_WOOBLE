using UnityEngine;
using UnityEngine.InputSystem; // Necesario para leer el control

public class PunteroApertura : MonoBehaviour
{
    [Header("Configuración")]
    public float distanciaRayo = 10f;
    public LayerMask capaBoton; // Para que solo detecte botones y no paredes
    public InputActionProperty botonDeAccion; // Aquí asignaremos el botón del mando (Trigger/Select)

    [Header("Debug Visual (Opcional)")]
    public bool mostrarRayoDebug = true;

    void Update()
    {
        // 1. Leemos si el botón (Gatillo) fue presionado en este frame
        // "WasPressedThisFrame" evita que se abra y cierre 20 veces si mantienes el botón
        if (botonDeAccion.action.WasPressedThisFrame())
        {
            DispararRayo();
        }
    }

    void DispararRayo()
    {
        RaycastHit hit;
        // Lanzamos un rayo desde la posición y rotación de este control hacia adelante
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanciaRayo, capaBoton))
        {
            // Verificamos si lo que tocamos tiene el script de la puerta
            PuertaLogica puerta = hit.collider.GetComponent<PuertaLogica>();
            
            if (puerta != null)
            {
                puerta.ActivarPuerta();
                Debug.Log("¡Puerta activada con rayo!");
            }
        }
    }
    
    // Dibuja una línea roja en la escena (Scene View) para ver a dónde apuntas
    void OnDrawGizmos()
    {
        if (mostrarRayoDebug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * distanciaRayo);
        }
    }
}