using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Pump : MonoBehaviour
{
    private Vector3 relativeOriginPos;
    public GameObject referencePoint;
    private Transform currentHand;
    public XRGrabInteractable xRGrab;
    public bool locked = true;
    private Vector3 defaultPos;
    public Vector3 rackBackMaxPos;

    void Start()
    {
        defaultPos = transform.localPosition;
    }
    public void SetFirstPos(SelectEnterEventArgs selectEvent)
    {
        if (xRGrab.interactorsSelecting.Count == 2)
        {
            currentHand = selectEvent.interactorObject.transform;
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            locked = false;
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
            float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
            displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
        }
    }

}
