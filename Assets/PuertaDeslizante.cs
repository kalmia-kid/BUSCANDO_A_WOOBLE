using UnityEngine;

public class ControladorPuerta : MonoBehaviour
{
    // Usamos un Animator porque es lo que suelen usar esos tutoriales
    // O si prefieres movimiento por código (más fácil si no sabes animar), usa esto:
    
    [Header("Configuración")]
    public Vector3 posicionAbiertaOffset = new Vector3(2, 0, 0); // Se mueve 2 metros en X
    public float velocidad = 3f;

    private Vector3 posInicial;
    private Vector3 posFinal;
    private bool estaAbierta = false;

    void Start()
    {
        posInicial = transform.localPosition;
        posFinal = transform.localPosition + posicionAbiertaOffset;
    }

    void Update()
    {
        // Movemos la puerta suavemente hacia su destino actual
        Vector3 destino = estaAbierta ? posFinal : posInicial;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, destino, velocidad * Time.deltaTime);
    }

    // ESTA es la función pública que el XR Interaction Toolkit buscará
    public void CambiarEstado()
    {
        estaAbierta = !estaAbierta;
    }
}