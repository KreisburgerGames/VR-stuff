using System;
using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pump : MonoBehaviour
{
    // Private variables
    private Vector3 relativeOriginPos;
    private Game game;
    private Animator nextShellAnimator;
    private Transform currentHand;
    private Vector3 defaultPos;
    public InteractorHandedness handedness;
    private bool chambered = false;
    private GameObject shellObj;
    private bool back = false;
    private float backDisplacement = 99f;
    // Public variables
    public XRGrabInteractable xRGrab;
    public GameObject referencePoint;
    public bool locked = true;
    public Vector3 rackBackMaxPos;
    public GameObject shellPrefab;
    public Mesh spentShellMesh;
    public Transform shellEjectPoint;
    public float ejectZ;
    public Transform maxHold, MinHold;
    public bool canShoot = false;
    public GameObject nextShell;

    void Start()
    {
        defaultPos = transform.localPosition;
        game = FindFirstObjectByType<Game>();
        nextShellAnimator = GetComponent<Animator>();
        nextShell.SetActive(false);
    }
    public void SetFirstPos(SelectEnterEventArgs selectEvent)
    {
        if (xRGrab.interactorsSelecting.Count == 2)
        {
            handedness = xRGrab.interactorsSelecting[0].handedness;
            currentHand = selectEvent.interactorObject.transform;
            // Set reference position for the pump in local space
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            // This next line will almost never happen but I wrote it in the earlier stages of making this and im not gonna touch it
            if(!chambered) locked = false;
        }
    }

    public void OnTriggerPull(ActivateEventArgs act)
    {
        // Only shoot with trigger finger while chambered and allowed to shoot
        if(act.interactorObject.handedness == handedness && chambered && canShoot)
        {
            canShoot = false;
            // Reset the reference position, makes it feel a little more natural
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            // Clamped between the edges of the pump
            relativeOriginPos.y = Mathf.Clamp(relativeOriginPos.y, MinHold.localPosition.z, maxHold.localPosition.z);
            if(shellObj != null)
            {
                shellObj.GetComponent<MeshFilter>().mesh = spentShellMesh;
                shellObj.GetComponent<Shell>().RevealShell();
            }
            if(!game.shellObjs[0].GetComponent<Shell>().blank) GetComponentInParent<ShotgunAnimation>().animator.Play("shotgun_fire");
            locked = false;
            chambered = false;
        }
    }
    
    public void Lock()
    {
        // take a wild fucking guess
        locked = true;
    }

    public void Eject()
    {
        // Unlock rigidbody and make sure everything works
        shellObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        shellObj.GetComponent<Rigidbody>().isKinematic = false;
        shellObj.GetComponent<Rigidbody>().useGravity = true;
        shellObj.GetComponent<BoxCollider>().enabled = true;
        // out.
        shellObj.transform.parent = null;
        // out^2.
        shellObj.GetComponent<Rigidbody>().AddForce(shellEjectPoint.right * UnityEngine.Random.Range(2f, 4f), ForceMode.Impulse);
        shellObj.GetComponent<Rigidbody>().AddTorque(shellEjectPoint.up * UnityEngine.Random.Range(-20f, 20f), ForceMode.Impulse);
        shellObj = null;
        // out^3.
        game.shellObjs.RemoveAt(0);
    }

    void Update()
    {
        if(!locked && xRGrab.interactorsSelecting.Count == 2)
        {
            if(!back)
            {
                // Get displacement from reference position set when grabbing pump and pulling trigger
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                // Clamp between the pump's minimum and maximum position
                displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
                // Make sure the next shell animation is reset
                nextShellAnimator.SetFloat("backPos", 0);
                if(shellObj != null && transform.localPosition.z <= ejectZ)
                {
                    // LEAVE
                    Eject();
                }
                if (MathF.Round(displacement, 5) == MathF.Round(defaultPos.z - rackBackMaxPos.z, 5))
                {
                    // I SAID LEAVE
                    if(shellObj != null) Eject();
                    back = true;
                }
                // Actually set the position
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
            }
            else
            {
                // Do the same fucking thing
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
                // BUT DIFFERENT! (Make sure the pump can only go forward)
                if (backDisplacement > displacement)
                {
                    backDisplacement = displacement;
                };
                displacement = Mathf.Clamp(displacement, 0, backDisplacement);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
                // Get info for next shell animation
                float currentPumpDisplacement = transform.localPosition.z - rackBackMaxPos.z;
                float maxDisplacement = rackBackMaxPos.x - rackBackMaxPos.z;
                // Animate
                nextShellAnimator.SetFloat("backPos", currentPumpDisplacement/maxDisplacement);
                if(MathF.Round(displacement, 5) == 0) // Copilot tried to get me to round 0 to the 5th digit, lemme uninstall I JUST remembered to
                {
                    back = false;
                    locked = true;
                    backDisplacement = 99f;
                    // Reset animation again JUST in case
                    nextShellAnimator.SetFloat("backPos", 0);
                    // Go back
                    game.state = Game.State.ReturnToPosition;
                }
            }
        }
    }

    public void Chamber()
    {
        // Reset state variables
        chambered = true;
        locked = true;
        back = false;
        backDisplacement = 99f;
        // Put the next shell in the chamber
        shellObj = game.shellObjs[0];
        shellObj.transform.parent = shellEjectPoint;
        shellObj.transform.localPosition = Vector3.zero;
        shellObj.transform.localEulerAngles = Vector3.zero;
        // Somehow it was inactive before idrk just set it active
        shellObj.SetActive(true);
        if(game.shellObjs.Count > 1) nextShell.SetActive(true); // Only if another shell is next
        else nextShell.SetActive(false);
    }
}
