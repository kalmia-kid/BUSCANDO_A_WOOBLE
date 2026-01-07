using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

public class CaptureDevice : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    [Tooltip("El punto en el dispositivo donde el alien debe quedar pegado (la punta).")]
    public Transform deviceAttachmentPoint; 
    
    [Tooltip("El nombre exacto del objeto dentro del Alien que debe coincidir con la punta.")]
    public string alienGrabPointName = "Wooble_Grab";

    [Header("Audio")] 
    public AudioClip captureSound;    // Sonido inicial (al tocar)
    public AudioClip absorptionSound; // Sonido intermedio (al absorber)
    public AudioClip notebookSound;   // NUEVO: Sonido final (al salir el notebook)
    
    private AudioSource audioSource;

    [Header("Efectos y Contención")]
    public GameObject containmentEffect;
    public GameObject absorptionEffect;
    public float anchorTimeBeforeDisappearance = 0.5f;
    public float absorptionEffectDuration = 0.1f;
    public Collider captureCollider;

    private void PlayEffect(GameObject effectObject)
    {
        if (effectObject == null) return;
        effectObject.SetActive(true);
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null && !ps.isPlaying) ps.Play(true);
        AudioSource effectAudio = effectObject.GetComponent<AudioSource>();
        if (effectAudio != null && !effectAudio.isPlaying) effectAudio.Play();
    }

    private void StopEffect(GameObject effectObject)
    {
        if (effectObject == null) return;
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        AudioSource effectAudio = effectObject.GetComponent<AudioSource>();
        if (effectAudio != null) effectAudio.Stop();
        effectObject.SetActive(false);
    }

    void Start()
    {
        StopEffect(containmentEffect);
        StopEffect(absorptionEffect);

        // CONFIGURACIÓN DE AUDIO
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; 
        }

        // CORRECCIÓN: EVITAR QUE SUENE AL DARLE AL PLAY
        audioSource.playOnAwake = false; 
        audioSource.Stop();              

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

        // REPRODUCIR SONIDO INICIAL (CAPTURE)
        if (audioSource != null && captureSound != null)
        {
            audioSource.PlayOneShot(captureSound);
        }

        if (captureCollider != null) captureCollider.enabled = false;

        wooble.DisablePhysicsAndMovement();

        // 2. ACTIVAR EFECTO Y ANCLAJE
        wooble.transform.SetParent(deviceAttachmentPoint);

        // LÓGICA DE SNAPPING
        Transform alienGrabPoint = FindDeepChild(wooble.transform, alienGrabPointName);

        if (alienGrabPoint != null)
        {
            Quaternion targetRotation = deviceAttachmentPoint.rotation * Quaternion.Inverse(alienGrabPoint.localRotation);
            wooble.transform.rotation = targetRotation;

            Vector3 positionOffset = alienGrabPoint.position - wooble.transform.position;
            wooble.transform.position = deviceAttachmentPoint.position - positionOffset;
        }
        else
        {
            Debug.LogWarning($"No se encontró el objeto '{alienGrabPointName}' dentro del Wooble. Usando posición 0.");
            wooble.transform.localPosition = Vector3.zero;
            wooble.transform.localRotation = Quaternion.identity;
        }

        PlayEffect(containmentEffect);

        yield return new WaitForSeconds(anchorTimeBeforeDisappearance);

        // 3. ABSORCIÓN FINAL
        StopEffect(containmentEffect);
        PlayEffect(absorptionEffect);

        // REPRODUCIR SONIDO DE ABSORCIÓN
        if (audioSource != null && absorptionSound != null)
        {
            audioSource.PlayOneShot(absorptionSound);
        }
        
        wooble.FinalizeDisappearance();

        yield return new WaitForSeconds(absorptionEffectDuration);
        StopEffect(absorptionEffect);

        // 4. REACTIVACIÓN Y FINAL DE MISIÓN
        if (captureCollider != null) captureCollider.enabled = true;

        // --- NUEVO: SONIDO DEL NOTEBOOK AGENT ---
        // Lo reproducimos justo antes de llamar al GameManager
        if (audioSource != null && notebookSound != null)
        {
            audioSource.PlayOneShot(notebookSound);
        }
        // ----------------------------------------

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionComplete();
        }
    }

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