using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class NextLevelOnGrab : MonoBehaviour
{
    [Header("Escena2")]
    public string nextSceneName;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
            Debug.LogWarning("NextLevelOnGrab: No se encontr� XRGrabInteractable en el mismo GameObject.");
    }

    void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
      
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("NextLevelOnGrab: nextSceneName est� vac�o. Especifica el nombre exacto de la escena en el inspector.");
            return;
        }

   
        SceneManager.LoadScene(nextSceneName);
    }
}

