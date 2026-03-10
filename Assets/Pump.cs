using System;
using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pump : MonoBehaviour
{
    private Vector3 relativeOriginPos;
    private Game game;
    public GameObject referencePoint;
    private Transform currentHand;
    public XRGrabInteractable xRGrab;
    public bool locked = true;
    private Vector3 defaultPos;
    public Vector3 rackBackMaxPos;
    private InteractorHandedness handedness;
    private bool chambered = false;
    private GameObject shellObj;
    public GameObject shellPrefab;
    public Mesh spentShellMesh;
    public Transform shellEjectPoint;
    public float ejectZ;
    public Transform maxHold, MinHold;
    private bool back = false;
    private float backDisplacement = 99f;
    public bool canShoot = false;
    private Animator nextShellAnimator;
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
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            if(!chambered) locked = false;
        }
    }

    public void OnTriggerPull(ActivateEventArgs act)
    {
        if(act.interactorObject.handedness == handedness && chambered && canShoot)
        {
            canShoot = false;
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            relativeOriginPos.y = Mathf.Clamp(relativeOriginPos.y, MinHold.localPosition.z, maxHold.localPosition.z);
            if(shellObj != null)
            {
                shellObj.GetComponent<MeshFilter>().mesh = spentShellMesh;
                shellObj.GetComponent<Shell>().RevealShell();
            }
            locked = false;
            chambered = false;
        }
    }
    
    public void Lock()
    {
        locked = true;
    }

    public void Eject()
    {
        shellObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        shellObj.GetComponent<Rigidbody>().isKinematic = false;
        shellObj.GetComponent<Rigidbody>().useGravity = true;
        shellObj.GetComponent<BoxCollider>().enabled = true;
        shellObj.transform.parent = null;
        shellObj.GetComponent<Rigidbody>().AddForce(shellEjectPoint.right * UnityEngine.Random.Range(2f, 4f), ForceMode.Impulse);
        shellObj.GetComponent<Rigidbody>().AddTorque(shellEjectPoint.up * UnityEngine.Random.Range(-20f, 20f), ForceMode.Impulse);
        shellObj = null;
        game.shellObjs.RemoveAt(0);
    }

    void Update()
    {
        if(!locked && xRGrab.interactorsSelecting.Count == 2)
        {
            if(!back)
            {
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
                nextShellAnimator.SetFloat("backPos", 0);
                if(shellObj != null && transform.localPosition.z <= ejectZ)
                {
                    Eject();
                }
                if (MathF.Round(displacement, 5) == MathF.Round(defaultPos.z - rackBackMaxPos.z, 5))
                {
                    if(shellObj != null) Eject();
                    back = true;
                }
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
            }
            else
            {
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
                if (backDisplacement > displacement)
                {
                    backDisplacement = displacement;
                };
                displacement = Mathf.Clamp(displacement, 0, backDisplacement);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
                float currentPumpDisplacement = transform.localPosition.z - rackBackMaxPos.z;
                float maxDisplacement = rackBackMaxPos.x - rackBackMaxPos.z;
                nextShellAnimator.SetFloat("backPos", currentPumpDisplacement/maxDisplacement);
                if(MathF.Round(displacement, 5) == 0)
                {
                    back = false;
                    locked = true;
                    backDisplacement = 99f;
                    nextShellAnimator.SetFloat("backPos", 0);
                    game.state = Game.State.ReturnToPosition;
                }
            }
        }
    }

    public void Chamber()
    {
        chambered = true;
        locked = true;
        back = false;
        backDisplacement = 99f;
        shellObj = game.shellObjs[0];
        shellObj.transform.parent = shellEjectPoint;
        shellObj.transform.localPosition = Vector3.zero;
        shellObj.transform.localEulerAngles = Vector3.zero;
        shellObj.SetActive(true);
        if(game.shellObjs.Count > 1) nextShell.SetActive(true);
        else nextShell.SetActive(false);
    }
}
