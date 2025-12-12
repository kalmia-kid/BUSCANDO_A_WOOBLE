using UnityEngine;
using System.Collections;
// Añadido para simplificar la referencia a XRGrabInteractable


public class CaptureDevice : MonoBehaviour
{
    [Header("Efectos y Contención")]
    [Tooltip("El GameObject que muestra el efecto visual de Wooble capturado (luz, destello, etc.).")]
    public GameObject containmentEffect;

    [Tooltip("Tiempo que Wooble permanece anclado antes de desaparecer (simula la absorción).")]
    public float anchorTimeBeforeDisappearance = 0.5f;

    [Tooltip("El Collider de esta trampa. Debe estar marcado como Is Trigger.")]
    public Collider captureCollider; // Es público, asumimos que se asigna en el Inspector

    // Eliminamos el Awake() ya que el Collider es público y se asigna externamente.

    void Start()
    {
        if (containmentEffect != null)
        {
            containmentEffect.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wooble"))
        {
            Wooble woobleToCapture = other.GetComponent<Wooble>();

            if (woobleToCapture != null && !woobleToCapture.IsCaptured)
            {
                // Usamos la referencia simplificada
                UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

                if (grabInteractable != null && grabInteractable.isSelected)
                {
                    StartCoroutine(CaptureSequence(woobleToCapture));
                }
            }
        }
    }

    /// <summary>
    /// Secuencia principal de captura: Anclaje -> Efecto -> Desaparición -> Activación de UI.
    /// </summary>
    private IEnumerator CaptureSequence(Wooble wooble)
    {
        // 1. INICIO DE LA CAPTURA Y ESTADO
        wooble.StartCapture();

        // Desactivamos el Collider de la trampa para evitar múltiples capturas
        if (captureCollider != null)
        {
            captureCollider.enabled = false;
        }

        // ANCLAJE PRECISO: Wooble se convierte en hijo del dispositivo.
        wooble.transform.SetParent(this.transform);

        // CORRECCIÓN CLAVE: Resetear la posición y rotación LOCAL para anclarse sin offset.
        wooble.transform.localPosition = Vector3.zero;
        wooble.transform.localRotation = Quaternion.identity;

        // 2. EFECTO VISUAL Y TIEMPO DE ESPERA
        if (containmentEffect != null)
        {
            containmentEffect.SetActive(true);
        }

        // Espera el tiempo de anclaje (muestra Wooble pegado a la trampa)
        yield return new WaitForSeconds(anchorTimeBeforeDisappearance);

        // 3. DESAPARICIÓN FINAL
        wooble.FinalizeDisappearance();

        // Desactiva el efecto visual
        if (containmentEffect != null)
        {
            containmentEffect.SetActive(false);
        }

        // 4. REACTIVACIÓN Y FIN DE MISIÓN

        // Volvemos a habilitar el Collider de la trampa
        if (captureCollider != null)
        {
            captureCollider.enabled = true;
        }

        // ¡LLAMADA CLAVE PARA LA UI! Llama a la función del GameManager para mostrar el menú de fin de nivel.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionComplete();
        }
    }
}