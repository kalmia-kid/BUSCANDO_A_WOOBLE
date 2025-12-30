using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Asegúrate de que esta referencia es correcta para tu versión de XR

public class CaptureDevice : MonoBehaviour
{
    [Header("Configuración de Agarre (NUEVO)")]
    [Tooltip("El punto en el dispositivo donde el alien debe quedar pegado (la punta).")]
    public Transform deviceAttachmentPoint; 
    
    [Tooltip("El nombre exacto del objeto dentro del Alien que debe coincidir con la punta.")]
    public string alienGrabPointName = "Wooble_Grab";

    [Header("Efectos y Contención")]
    public GameObject containmentEffect;
    public GameObject absorptionEffect;
    public float anchorTimeBeforeDisappearance = 0.5f;
    public float absorptionEffectDuration = 0.1f;
    public Collider captureCollider;

    // ... (Funciones PlayEffect y StopEffect se mantienen igual) ...
    private void PlayEffect(GameObject effectObject)
    {
        if (effectObject == null) return;
        effectObject.SetActive(true);
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null && !ps.isPlaying) ps.Play(true);
        AudioSource audioSource = effectObject.GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
    }

    private void StopEffect(GameObject effectObject)
    {
        if (effectObject == null) return;
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        AudioSource audioSource = effectObject.GetComponent<AudioSource>();
        if (audioSource != null) audioSource.Stop();
        effectObject.SetActive(false);
    }

    void Start()
    {
        StopEffect(containmentEffect);
        StopEffect(absorptionEffect);

        // Si no has asignado el punto de agarre en el inspector, usamos el propio transform del objeto como "punta" por defecto
        if (deviceAttachmentPoint == null)
        {
            deviceAttachmentPoint = this.transform;
            Debug.LogWarning("CaptureDevice: No has asignado 'Device Attachment Point'. Usando el centro del objeto por defecto.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wooble"))
        {
            Wooble woobleToCapture = other.GetComponent<Wooble>();

            if (woobleToCapture != null && !woobleToCapture.IsCaptured)
            {
                XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
                // Comprobamos si el jugador tiene el arma agarrada (opcional, según tu diseño)
                if (grabInteractable != null && grabInteractable.isSelected)
                {
                    StartCoroutine(CaptureSequence(woobleToCapture));
                }
            }
        }
    }

    private IEnumerator CaptureSequence(Wooble wooble)
    {
        // 1. INICIO DE LA CAPTURA
        wooble.StartCapture();

        if (captureCollider != null) captureCollider.enabled = false;

        wooble.DisablePhysicsAndMovement();

        // 2. ACTIVAR EFECTO Y ANCLAJE (LÓGICA MEJORADA)
        
        // Primero hacemos hijo al alien del punto de anclaje
        wooble.transform.SetParent(deviceAttachmentPoint);

        // --- INICIO LÓGICA DE SNAPPING (Wooble_Grab a DeviceAttachmentPoint) ---
        
        // Buscamos el punto de agarre DENTRO del alien
        Transform alienGrabPoint = FindDeepChild(wooble.transform, alienGrabPointName);

        if (alienGrabPoint != null)
        {
            // A. ALINEAR ROTACIÓN
            // Rotamos el alien para que la rotación del grab point coincida con la del attachment point.
            // La fórmula es: RotaciónDestino * Inversa(RotaciónLocalDelHijo)
            Quaternion targetRotation = deviceAttachmentPoint.rotation * Quaternion.Inverse(alienGrabPoint.localRotation);
            wooble.transform.rotation = targetRotation;

            // B. ALINEAR POSICIÓN
            // Ahora que la rotación es correcta, calculamos la diferencia de posición
            // Queremos que alienGrabPoint.position sea igual a deviceAttachmentPoint.position
            Vector3 positionOffset = alienGrabPoint.position - wooble.transform.position;
            wooble.transform.position = deviceAttachmentPoint.position - positionOffset;
        }
        else
        {
            // Fallback: Si no encuentra "Wooble_Grab", lo pega al centro como antes
            Debug.LogWarning($"No se encontró el objeto '{alienGrabPointName}' dentro del Wooble. Usando posición 0.");
            wooble.transform.localPosition = Vector3.zero;
            wooble.transform.localRotation = Quaternion.identity;
        }
        // --- FIN LÓGICA DE SNAPPING ---

        PlayEffect(containmentEffect);

        yield return new WaitForSeconds(anchorTimeBeforeDisappearance);

        // 3. ABSORCIÓN FINAL
        StopEffect(containmentEffect);
        PlayEffect(absorptionEffect);
        
        wooble.FinalizeDisappearance();

        yield return new WaitForSeconds(absorptionEffectDuration);
        StopEffect(absorptionEffect);

        // 4. REACTIVACIÓN
        if (captureCollider != null) captureCollider.enabled = true;
        if (GameManager.Instance != null) GameManager.Instance.OnMissionComplete();
    }

    // Función auxiliar para buscar hijos en profundidad (por si Wooble_Grab está dentro de otros huesos)
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}