using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Highlight : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public MeshRenderer meshRenderer;
    private Material material;
    public float highlightIntensity = 0.07f; // Intensity of the highlight

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        material = meshRenderer.material;
        material.EnableKeyword("_EMISSION");
    }

    public void OnFirstHoverEntered(HoverEnterEventArgs args)
    {
        if(grabInteractable.interactorsSelecting.Count == 0)
            material.SetColor("_EmissionColor", new Color(highlightIntensity, highlightIntensity, highlightIntensity));
    }

    public void OnFirstHoverExited(HoverExitEventArgs args)
    {
        // Revert the material color to original
        material.SetColor("_EmissionColor", Color.black);
    }
}
