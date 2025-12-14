using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CaptureDevice : MonoBehaviour
{
    [Header("Efectos y Contención")]
    public GameObject containmentEffect;
    public GameObject absorptionEffect;
    public float anchorTimeBeforeDisappearance = 0.5f;
    public float absorptionEffectDuration = 0.1f;
    public Collider captureCollider;

    // Función auxiliar para forzar la reproducción de efectos
    private void PlayEffect(GameObject effectObject)
    {
        if (effectObject == null) return;

        // 1. Activar el GameObject
        effectObject.SetActive(true);

        // 2. Forzar la reproducción de sistemas de partículas
        // Es común que los sistemas de partículas no se reproduzcan con solo activar el objeto.
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null && !ps.isPlaying)
        {
            ps.Play(true); // 'true' incluye los hijos
        }

        // 3. Forzar la reproducción de audio (si el efecto tiene sonido)
        AudioSource audioSource = effectObject.GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // 4. Forzar la reproducción de animaciones (si el efecto es una animación)
        // Nota: Los Animators a menudo funcionan bien con SetActive, pero si falla, 
        // podrías necesitar llamar a animator.Play("nombre_del_clip").
    }

    // Función auxiliar para detener/ocultar efectos
    private void StopEffect(GameObject effectObject)
    {
        if (effectObject == null) return;

        // Detener partículas
        ParticleSystem ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Detener audio
        AudioSource audioSource = effectObject.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        effectObject.SetActive(false);
    }


    void Start()
    {
        // Usamos la nueva función para asegurar que inician apagados.
        StopEffect(containmentEffect);
        StopEffect(absorptionEffect);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ... (Lógica de OnTriggerEnter, se mantiene igual) ...
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

    /// <summary>
    /// Secuencia principal de captura con efectos sostenidos y finales.
    /// </summary>
    private IEnumerator CaptureSequence(Wooble wooble)
    {
        // 1. INICIO DE LA CAPTURA Y ESTADO
        wooble.StartCapture();

        if (captureCollider != null)
        {
            captureCollider.enabled = false;
        }

        wooble.DisablePhysicsAndMovement();

        // 2. ACTIVAR EFECTO DE CONTENCIÓN Y ANCLAJE (SOSTENIDO)
        wooble.transform.SetParent(this.transform);
        wooble.transform.localPosition = Vector3.zero;
        wooble.transform.localRotation = Quaternion.identity;

        // Activa y fuerza la reproducción del efecto de Contención Sostenido
        PlayEffect(containmentEffect);

        // Espera el tiempo de anclaje (Wooble se ve pegado y contenido)
        yield return new WaitForSeconds(anchorTimeBeforeDisappearance);

        // 3. EFECTO DE ABSORCIÓN FINAL Y DESAPARICIÓN

        // 3a. Detener/Ocultar el efecto de Contención Sostenido
        StopEffect(containmentEffect);

        // 3b. Activar y forzar la reproducción del efecto Final de Absorción
        PlayEffect(absorptionEffect);

        // 3c. Desaparición de Wooble
        wooble.FinalizeDisappearance();

        // 3d. Espera breve para el efecto Final
        yield return new WaitForSeconds(absorptionEffectDuration);

        // 3e. Desactivar el efecto Final
        StopEffect(absorptionEffect);

        // 4. REACTIVACIÓN Y FIN DE MISIÓN
        if (captureCollider != null)
        {
            captureCollider.enabled = true;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionComplete();
        }
    }
}