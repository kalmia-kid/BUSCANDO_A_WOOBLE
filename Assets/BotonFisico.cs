using UnityEngine;
using System.Collections;

public class BotonFisico : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public Transform puertaObjeto; // Arrastra aquí el objeto puerta (el hijo, no el padre si tiene offsets)
    public Vector3 posicionAbiertaOffset = new Vector3(0, 2, 0); // Cuánto se mueve (ej. 2 metros arriba)
    public float velocidad = 2.0f;
    
    [Header("Configuración de Interacción")]
    public string tagDeLaMano = "Player"; // Importante: Debes poner este Tag a tus manos/controles

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;
    private bool estaAbierta = false;
    private Coroutine animacionActual;

    void Start()
    {
        if (puertaObjeto == null)
        {
            Debug.LogError("¡No has asignado el objeto puerta en el inspector!");
            return;
        }
        
        posicionCerrada = puertaObjeto.localPosition; // Usamos localPosition para evitar problemas de rotación del padre
        posicionAbierta = posicionCerrada + posicionAbiertaOffset;
    }

    // Esta función nativa de Unity detecta cuando algo entra en el botón
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que tocó el botón es la mano (usando el Tag)
        if (other.CompareTag(tagDeLaMano))
        {
            TogglePuerta();
        }
    }

    void TogglePuerta()
    {
        if (estaAbierta)
            MoverA(posicionCerrada);
        else
            MoverA(posicionAbierta);
        
        estaAbierta = !estaAbierta;
    }

    void MoverA(Vector3 destino)
    {
        if (animacionActual != null) StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(MoverCorrutina(destino));
    }

    IEnumerator MoverCorrutina(Vector3 destino)
    {
        while (Vector3.Distance(puertaObjeto.localPosition, destino) > 0.001f)
        {
            puertaObjeto.localPosition = Vector3.MoveTowards(puertaObjeto.localPosition, destino, velocidad * Time.deltaTime);
            yield return null;
        }
        puertaObjeto.localPosition = destino;
    }
}