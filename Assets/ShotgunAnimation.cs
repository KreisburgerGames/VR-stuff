using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ShotgunAnimation : MonoBehaviour
{
    [SerializeField] private InputActionReference triggerActionReference;
    public GameObject rightHandPrimary, leftHandSecondary;
    public GameObject rightController, leftController;
    private XRGrabInteractable grab;
    public Pump pump;
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        grab = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        float triggerValue = triggerActionReference.action.ReadValue<float>();
        animator.SetFloat("Trigger", triggerValue);
    }

    // Only right handed supported currently
    public void Selected(SelectEnterEventArgs args)
    {
        if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            rightHandPrimary.SetActive(true);
            rightController.SetActive(false);
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            leftHandSecondary.SetActive(true);
            leftController.SetActive(false);
        }
        animator.enabled = true;
    }

    private void Drop()
    {
        rightHandPrimary.SetActive(false);
        rightController.SetActive(true);
        leftHandSecondary.SetActive(false);
        leftController.SetActive(true);
        grab.enabled = false;
        grab.enabled = true;
        animator.enabled = false;
    }

    public void Deselected(SelectExitEventArgs args)
    {
        if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            rightHandPrimary.SetActive(false);
            rightController.SetActive(true);
            if(pump.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right && grab.interactorsSelecting.Count == 1) Drop(); 
            if(grab.interactorsSelecting.Count == 0) animator.enabled = false;
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            leftHandSecondary.SetActive(false);
            leftController.SetActive(true);
            if(pump.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left && grab.interactorsSelecting.Count == 1) Drop(); 
            if(grab.interactorsSelecting.Count == 0) animator.enabled = false;
        }
    }
}
