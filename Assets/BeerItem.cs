using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BeerItem : MonoBehaviour
{
    private Pump pump;
    private Item item;
    private float liquidLevel = 100f;
    private XRGrabInteractable xRGrab;
    public float drinkSpeed = 10f;
    public float minRotation, maxRotation;
    public LayerMask whatIsPlayer;
    private bool opened = false;
    private bool used = false;
    public GameObject drinkingParticle;

    void Start()
    {
        pump = FindFirstObjectByType<Pump>();
        item = GetComponent<Item>();
        xRGrab = GetComponent<XRGrabInteractable>();
    }

    public void Open()
    {
        if(!item.canUse) return;
        opened = true;
    }

    void Update()
    {
        if(item.canUse && !used && xRGrab.interactorsSelecting.Count == 1)
        {
            bool rayHitsSelf = false;
            Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 5f, whatIsPlayer);
            if(hit.collider.gameObject != null && hit.collider.gameObject.tag == "Self") rayHitsSelf = true;
            if(transform.rotation.x > minRotation && transform.rotation.x < maxRotation && rayHitsSelf)
            {
                liquidLevel -= drinkSpeed * Time.deltaTime;
                if(!drinkingParticle.activeSelf) drinkingParticle.SetActive(true);
                if(liquidLevel <= 0)
                {
                    used = true;
                    pump.UnlockForSkip();
                }
            } else if(drinkingParticle.activeSelf) drinkingParticle.SetActive(false);
        } else if(drinkingParticle.activeSelf) drinkingParticle.SetActive(false);
    }
}
