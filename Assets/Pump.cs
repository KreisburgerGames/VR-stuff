using System;
using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pump : MonoBehaviour
{
    private Vector3 relativeOriginPos;
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
    private bool back = false;
    private float backDisplacement = 99f;

    void Start()
    {
        defaultPos = transform.localPosition;
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
        if(act.interactorObject.handedness == handedness && chambered)
        {
            print("shoot");
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            if(shellObj != null)
            {
                shellObj.GetComponent<MeshFilter>().mesh = spentShellMesh;
            }
            locked = false;
            chambered = false;
        }
    }
    
    public void Lock()
    {
        locked = true;
    }

    void Update()
    {
        if(!locked)
        {
            if(!back)
            {
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
                print(displacement + " " + (defaultPos.z - rackBackMaxPos.z));
                if (MathF.Round(displacement, 5) == MathF.Round(defaultPos.z - rackBackMaxPos.z, 5))
                {
                    back = true;
                    if(shellObj != null)
                    {
                        shellObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                        shellObj.GetComponent<Rigidbody>().isKinematic = false;
                        shellObj.GetComponent<Rigidbody>().useGravity = true;
                        shellObj.GetComponent<BoxCollider>().enabled = true;
                        shellObj.transform.parent = null;
                        shellObj.GetComponent<Rigidbody>().AddForce(shellEjectPoint.right * 4f, ForceMode.Impulse);
                        shellObj.GetComponent<Rigidbody>().AddTorque(shellEjectPoint.up * UnityEngine.Random.Range(-2f, 2f), ForceMode.Impulse);
                    }
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
                if(MathF.Round(displacement, 5) == 0)
                {
                    back = false;
                    locked = true;
                    backDisplacement = 99f;
                    if (!chambered)
                    {
                        chambered = true;
                        shellObj = Instantiate(shellPrefab, transform.position, transform.rotation);
                        shellObj.transform.parent = shellEjectPoint;
                        shellObj.transform.localPosition = Vector3.zero;
                        shellObj.transform.localEulerAngles = Vector3.zero;
                    }
                }
            }
        }
    }

}
