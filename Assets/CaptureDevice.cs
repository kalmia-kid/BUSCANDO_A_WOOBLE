using UnityEngine;
using System.Collections;


public class CaptureDevice : MonoBehaviour
{
    [Header("Efectos y Contención")]
    [Tooltip("El GameObject que muestra el efecto visual de Wooble capturado (luz, destello, etc.).")]
    public GameObject containmentEffect;

    [Tooltip("Tiempo que Wooble permanece anclado antes de desaparecer (simula la absorción).")]
    public float anchorTimeBeforeDisappearance = 0.5f; // NUEVO TIEMPO para anclaje

    public Collider captureCollider;

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
                UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

                if (grabInteractable != null && grabInteractable.isSelected)
                {
                    StartCoroutine(CaptureSequence(woobleToCapture));
                }
            }
        }
    }

    /// <summary>
    /// Secuencia principal de captura: Anclaje -> Efecto -> Desaparición.
    /// </summary>
    private IEnumerator CaptureSequence(Wooble wooble)
    {
        // 1. INICIO DE LA CAPTURA Y ANCLAJE
        wooble.StartCapture();

        // Desactivamos el Collider de la trampa para evitar múltiples capturas
        if (captureCollider != null)
        {
            captureCollider.enabled = false;
        }

        // ANCLAJE: Wooble se convierte en hijo del dispositivo (se pega a él)
        wooble.transform.SetParent(this.transform);

        // 2. EFECTO VISUAL
        if (containmentEffect != null)
        {
            containmentEffect.SetActive(true);
        }

        // Espera el tiempo de anclaje (mostrando que está siendo absorbido)
        yield return new WaitForSeconds(anchorTimeBeforeDisappearance);

        // 3. DESAPARICIÓN FINAL

        // Llama a Wooble para que se desactive
        wooble.FinalizeDisappearance();

        // Desactiva el efecto visual
        if (containmentEffect != null)
        {
            containmentEffect.SetActive(false);
        }

        // 4. REACTIVACIÓN
        // Volvemos a habilitar el Collider para futuras interacciones
        if (captureCollider != null)
        {
            captureCollider.enabled = true;
        }

        // Opcional: GameManager.Instance.OnWoobleCaptured();
    }
}