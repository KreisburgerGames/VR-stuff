using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ShotgunAnimation : MonoBehaviour
{
    [SerializeField] private InputActionReference triggerActionReference;
    public GameObject rightHand, leftHand;
    public GameObject rightController, leftController;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
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
            rightHand.SetActive(true);
            rightController.SetActive(false);
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            leftHand.SetActive(true);
            leftController.SetActive(false);
        }
    }

    public void Deselected(SelectExitEventArgs args)
    {
        if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            rightHand.SetActive(false);
            rightController.SetActive(true);
        }
        else if(args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            leftHand.SetActive(false);
            leftController.SetActive(true);
        }
    }
}
