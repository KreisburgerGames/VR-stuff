using System;
using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.SceneManagement;
using System.Net.NetworkInformation;
using System.Collections;
using System.Threading;

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
    public LayerMask hittableLayer;
    private bool blankSelf = false;
    public Transform primaryGrabPos, offsethandGrabPos;
    public InputActionReference offsetButton;
    private bool checking = false;
    private bool skipping = false;
    public GameObject barrelEnd;
    public bool barrelCut = false;
    public Transform bodyTransform;

    void Start()
    {
        defaultPos = transform.localPosition;
        game = FindFirstObjectByType<Game>();
        nextShellAnimator = GetComponent<Animator>();
        nextShell.SetActive(false);
        xRGrab.attachTransform = primaryGrabPos;
        offsetButton.action.started += ToggleGrabPos;
    }

    private IEnumerator FadeOutBarrel()
    {
        yield return new WaitForSeconds(2f);
        float timer = 0f;
        float fadeDuration = 1f;
        Material barrelMaterial = barrelEnd.GetComponent<MeshRenderer>().material;
        while (timer < fadeDuration)
        {
            Color color = barrelMaterial.color;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            barrelMaterial.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        barrelEnd.SetActive(false);
    }

    public void CutBarrel()
    {
        if(barrelCut) return;
        barrelCut = true;
        barrelEnd.transform.parent = null;
        barrelEnd.AddComponent<Rigidbody>();
        barrelEnd.GetComponent<CapsuleCollider>().enabled = true;
        StartCoroutine(FadeOutBarrel());
    }

    public IEnumerator ResetBarrel()
    {
        barrelCut = false;
        barrelEnd.transform.parent = bodyTransform;
        barrelEnd.transform.localPosition = Vector3.zero;
        barrelEnd.transform.localEulerAngles = Vector3.zero;
        barrelEnd.GetComponent<CapsuleCollider>().enabled = false;
        Destroy(barrelEnd.GetComponent<Rigidbody>());
        float timer = 0f;
        float fadeDuration = 1f;
        Material barrelMaterial = barrelEnd.GetComponent<MeshRenderer>().material;
        yield return new WaitForSeconds(1f);
        barrelEnd.SetActive(true);
        while (timer < fadeDuration)
        {
            Color color = barrelMaterial.color;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            barrelMaterial.color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
    }
    
    public void UnlockForCheck()
    {
        checking = true;
        shellObj.GetComponent<Shell>().RevealShell();
    }

    public void UnlockForSkip()
    {
        skipping = true;
        shellObj.GetComponent<Shell>().RevealShell();
    }

    void OnDestroy()
    {
        offsetButton.action.started -= ToggleGrabPos;
    }

    public void ToggleGrabPos(InputAction.CallbackContext ctx)
    {
        print(xRGrab.interactorsSelecting.Count);
        if(xRGrab.interactorsSelecting.Count != 1) return; // Only toggle if the pump is currently held
        if(xRGrab.attachTransform == primaryGrabPos)
        {
            xRGrab.attachTransform = offsethandGrabPos;
        }
        else
        {
            xRGrab.attachTransform = primaryGrabPos;
        }
        IXRSelectInteractor xRInteractor = xRGrab.interactorsSelecting[0];
        xRGrab.enabled = false;
        xRGrab.enabled = true;
        FindFirstObjectByType<XRInteractionManager>().SelectEnter(xRInteractor, xRGrab);
    }

    public void SetFirstPos(SelectEnterEventArgs selectEvent)
    {
        if (xRGrab.interactorsSelecting.Count == 2) // Check for offhand grab
        {
            handedness = xRGrab.interactorsSelecting[0].handedness;
            currentHand = selectEvent.interactorObject.transform;
            // Set reference position for the pump in local space
            relativeOriginPos = referencePoint.transform.InverseTransformPoint(currentHand.position);
            // Unlocks the chamber if player shot, didn't cycle and dropped the gun
            if(!chambered) locked = false;
        }
    }

    public void OnTriggerPull(ActivateEventArgs act)
    {
        // Only shoot with trigger finger while chambered and allowed to shoot
        if(act.interactorObject.handedness == handedness && chambered && canShoot && !checking && !skipping)
        {
            if(Physics.Raycast(shellEjectPoint.position, shellEjectPoint.forward, out RaycastHit hit, 5f, hittableLayer, QueryTriggerInteraction.Collide))
            {
                if(hit.collider.gameObject.tag == "Self")
                {
                    if(game.shellObjs[0].GetComponent<Shell>().blank)
                    {
                        blankSelf = true;
                    }
                    else
                    {
                        game.playerLives -= 1;
                        if(barrelCut) game.playerLives -= 1; // Extra damage if the barrel is cut
                        if (game.playerLives == 0) UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    }
                }
                else if(hit.collider.gameObject.tag == "Dealer")
                {
                    game.enemyLives -= 1;
                    if(barrelCut) game.enemyLives -= 1;
                }
            }
            else return;
            game.DisableItems();
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
        // Used in the deselect XR Grabbable Event
        locked = true;
    }

    public void Eject()
    {
        // Unlock rigidbody and make sure everything works
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
        if((!locked || checking || skipping) && xRGrab.interactorsSelecting.Count == 2)
        {
            if(!back)
            {
                // Get displacement from reference position set when grabbing pump and pulling trigger
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                // Clamp between the pump's minimum and maximum position
                if(!checking) displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z); else displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.y);
                // Make sure the next shell animation is reset
                nextShellAnimator.SetFloat("backPos", 0);
                if(shellObj != null && transform.localPosition.z <= ejectZ)
                {
                    Eject();
                }
                if(!checking)
                {
                   if (MathF.Round(displacement, 5) == MathF.Round(defaultPos.z - rackBackMaxPos.z, 5))
                    {
                        if(shellObj != null) Eject();
                        back = true;
                    } 
                }
                else
                {
                    if (MathF.Round(displacement, 5) == MathF.Round(defaultPos.z - rackBackMaxPos.y, 5))
                    {
                        back = true;
                    }
                }
                // Update position
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
            }
            else
            {
                float displacement = referencePoint.transform.InverseTransformPoint(currentHand.position).y - relativeOriginPos.y;
                if(!checking) displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.z); else displacement = Mathf.Clamp(displacement, 0, defaultPos.z - rackBackMaxPos.y);
                // Same code, but it doesn't allow the user to bring the pump back again
                if (backDisplacement > displacement)
                {
                    backDisplacement = displacement;
                };
                displacement = Mathf.Clamp(displacement, 0, backDisplacement);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, defaultPos.z - displacement);
                // Get info for next shell animation
                if(!checking)
                {
                    float currentPumpDisplacement = transform.localPosition.z - rackBackMaxPos.z;
                    float maxDisplacement = rackBackMaxPos.x - rackBackMaxPos.z;
                    // Animate
                    nextShellAnimator.SetFloat("backPos", currentPumpDisplacement/maxDisplacement);
                }
                if(MathF.Round(displacement, 5) == 0)
                {
                    back = false;
                    locked = true;
                    backDisplacement = 99f;
                    // Reset animation for next time
                    nextShellAnimator.SetFloat("backPos", 0);
                    // Go back
                    
                    if(checking) {checking = false; return;}
                    else if (skipping)
                    {
                        if(game.shellObjs.Count > 0)
                        {
                            skipping = false;
                            Chamber();
                            return;
                        }
                    }
                    
                    else if(game.enemyLives > 0)
                    {
                        if(blankSelf || skipping)
                            game.state = Game.State.ReturnOnBlank;
                        else
                            game.state = Game.State.ReturnToPosition;
                        skipping = false;
                    }
                    else
                    {
                        game.state = Game.State.ReturnThenNextStage;
                    }
                    blankSelf = false;
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
        shellObj.SetActive(true);
        if(game.shellObjs.Count > 1) nextShell.SetActive(true); // Only if there is another shell after the current one
        else nextShell.SetActive(false);
    }
}
