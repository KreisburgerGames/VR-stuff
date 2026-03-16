using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ItemBox : MonoBehaviour
{
    public XRInteractionManager interactionManager;
    private bool subscribed = false;
    private GameObject interactorHovered; // Used to check if the interactor that exited a hover is the original interactor that entered it
    public GameObject nextObject {get; private set;}
    public Game game;

    public void SetNext(GameObject newObject)
    {
        nextObject = newObject;
    }

    private void OnGrab(InputAction.CallbackContext ctx)
    {
        nextObject.transform.position = transform.position; // XR Grab Interactable is velocity tracking, so this avoids any collision issues when you force the select interaction
        nextObject.GetComponent<Rigidbody>().isKinematic = false;
        game.playerItems.Add(nextObject);
        interactionManager.SelectEnter((IXRSelectInteractor)interactorHovered.GetComponent<NearFarInteractor>(), nextObject.GetComponent<XRGrabInteractable>());
        nextObject = null;
        // Unsubscribe from the grab event and reset variables
        ctx.action.performed -= (ctx) => OnGrab(ctx);
        subscribed = false;
        interactorHovered = null;
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.name)
;        try
        {
            if(other.gameObject.GetComponent<NearFarInteractor>().interactablesSelected.Count == 0 && !subscribed && nextObject != null) // Only register the hover if nothing is already held and the other hand is not hovering, and if there is an object to give
            {
                print("yes");
                other.gameObject.GetComponent<NearFarInteractor>().selectInput.inputActionReferencePerformed.action.started += (ctx) => OnGrab(ctx);
                interactorHovered = other.gameObject;
                subscribed = true;
            }
        }
        catch // A different game object entered the trigger (Do nothing)
        {
            return;
        }
    }

    void OnTriggerExit(Collider other)
    {
        try
        {
            if(interactorHovered == other.gameObject && subscribed) // Check to see if it is the original hand that hovered and that the script is subscribed to the select/grab event
            {
                // Unsubscribe and reset variables (Will not be done in the OnGrab() event if player doesn't actually GRAB the item)
                other.gameObject.GetComponent<NearFarInteractor>().selectInput.inputActionValue.started -= (ctx) => OnGrab(ctx);
                interactorHovered = null;
                subscribed = false;
            }
        }
        catch
        {
            return;
        }
    }
}
