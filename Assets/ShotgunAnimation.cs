using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ShotgunAnimation : MonoBehaviour
{
    [SerializeField] private InputActionReference triggerActionReferenceRight, triggerActionReferenceLeft;
    public GameObject rightHandPrimary, rightHandSecondary, leftHandPrimary, leftHandSecondary;
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
        float triggerValue = 0f;
        if(rightHandPrimary.activeSelf)
            triggerValue = triggerActionReferenceRight.action.ReadValue<float>();
        else if(leftHandPrimary.activeSelf)
            triggerValue = triggerActionReferenceLeft.action.ReadValue<float>();
        animator.SetFloat("Trigger", triggerValue);
    }

    public void Selected(SelectEnterEventArgs args)
    {
        // Check for handedness and evaluate if it is the primary or secondary hand
        if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            if(grab.interactorsSelecting.Count < 2)
            {
                // Make sure animator works for both hands
                animator.Play("RightFire");
                animator.SetBool("RightHanded", true);
                rightHandPrimary.SetActive(true);
                rightController.SetActive(false);
            }
            else
            {
                rightHandSecondary.SetActive(true);
                rightController.SetActive(false);
            }
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            if(grab.interactorsSelecting.Count < 2)
            {
                animator.Play("LeftFire");
                animator.SetBool("RightHanded", false);
                leftHandPrimary.SetActive(true);
                leftController.SetActive(false);
            }
            else
            {
                leftHandSecondary.SetActive(true);
                leftController.SetActive(false);
            }
        }
        animator.enabled = true;
    }

    private void Drop()
    {
        // Force player to drop
        rightHandPrimary.SetActive(false); rightHandSecondary.SetActive(false);
        rightController.SetActive(true);
        leftHandSecondary.SetActive(false); leftHandSecondary.SetActive(false);
        leftController.SetActive(true);
        grab.enabled = false;
        grab.enabled = true;
        animator.enabled = false;
    }

    public void Deselected(SelectExitEventArgs args)
    {
        if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            // Disable both right/left hand objects (Depending of the if statement above), will work if it was the primary or secondary hand
            rightHandPrimary.SetActive(false); rightHandSecondary.SetActive(false);
            rightController.SetActive(true);
            // Force a drop if the pump is still held with the secondary hand
            if(pump.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right && grab.interactorsSelecting.Count == 1) Drop(); 
            if(grab.interactorsSelecting.Count == 0) animator.enabled = false; // Disable animator 
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            leftHandSecondary.SetActive(false); leftHandPrimary.SetActive(false);
            leftController.SetActive(true);
            if(pump.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left && grab.interactorsSelecting.Count == 1) Drop(); 
            if(grab.interactorsSelecting.Count == 0) animator.enabled = false;
        }
    }
}
