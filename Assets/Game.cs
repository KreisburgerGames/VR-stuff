using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Game : MonoBehaviour
{
    public GameObject shotgun;
    private XRGrabInteractable shotgunGrab;
    public Transform ShotgunDefaultPos;
    public State state = State.Wait;
    public int round = 1;
    public int stage = 1;
    public int shellsMin, shellsMax;
    public Transform shellsPoint;
    public float shellsOffset;
    public List<GameObject> shellObjs = new List<GameObject>();
    private State oldState = State.Wait;
    public GameObject shellPrefab;
    public Pump pump;
    public List<GameObject> enemyItems = new List<GameObject>();
    public int blankShellsLeft, liveShellsLeft;
    public bool doesHeKnow = false;
    public Enemy enemy;
    public BoxCollider sgBodyCol, sgPumpCol; // Both attachment transform colliders on the shotgun

    public enum State
    {
        Start,
        LoadingShells,
        PlayerShooting,
        EnemyShooting,
        ReturnToPosition,
        ReturnOnBlank,
        ItemsGiving,
        Wait
    }

    void Start()
    {
        shotgunGrab = shotgun.GetComponent<XRGrabInteractable>();
        shotgunGrab.enabled = false;
    }

    IEnumerator ShowShells()
    {
        // Shells are currently shown when it reaches here, allow 3 seconds for player to count shells
        yield return new WaitForSeconds(3f);
        foreach(GameObject shell in shellObjs)
        {
            // "Invisible"
            shell.transform.position = new Vector3(shell.transform.position.x, shell.transform.position.y - 5f, shell.transform.position.z);
            shell.GetComponent<Shell>().Hide(); // Sets their base color on the material to black which hides if their blank or not
        }
        GameObject[] s = (GameObject[])ArrayExtensionMethods.Shuffle(shellObjs.ToArray());
        shellObjs = s.ToList();
        state = State.PlayerShooting;
    }

    void Update()
    {
        if(state != oldState) // Only runs ONCE per state change
        {
            oldState = state;
            if(state == State.Start)
            {
                // Choose between 4 and 8 shells but slightly influenced off what round it is
                int min = shellsMin + round; int max = shellsMax + round;
                min = Mathf.Clamp(min, 3, 4);
                max = Mathf.Clamp(max, 4, 8);
                int shellGen = Random.Range(min, max + 1);
                for(int i = 0; i < shellGen; i++)
                {
                    GameObject shell = Instantiate(shellPrefab, shellsPoint.position, Quaternion.identity);
                    if (Random.Range(0, 2) == 0) shell.GetComponent<Shell>().blank = true;
                    shell.transform.parent = shellsPoint;
                    shell.transform.position = new Vector3(shell.transform.position.x + shellsOffset * i, shell.transform.position.y, shell.transform.position.z);
                    shellObjs.Add(shell);
                }
                // Ensure at least one shell is live
                bool oneIsLive = false;
                foreach(GameObject shell in shellObjs)
                {
                    if(!shell.GetComponent<Shell>().blank)
                    {
                        oneIsLive = true;
                        break;
                    }
                }
                if(!oneIsLive)
                {
                    shellObjs[Random.Range(0, shellObjs.Count)].GetComponent<Shell>().blank = false;
                }
                StartCoroutine(ShowShells());
                state = State.Wait;
                pump.nextShell.SetActive(true);
            }
            if(state == State.LoadingShells)
            {
                
            }
            else if(state == State.PlayerShooting)
            {
                // State can be landed on with 0 shells
                if(shellObjs.Count == 0)
                {
                    StartCoroutine(ReturnToPosition(State.ItemsGiving));
                    return;
                }
                // Prepare the shotgun for the player's turn
                pump.Chamber();
                shotgunGrab.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                shotgunGrab.gameObject.GetComponent<BoxCollider>().enabled = true;
                sgBodyCol.enabled = true;
                sgPumpCol.enabled = true;
                shotgunGrab.enabled = true;
                pump.canShoot = true;
            }
            else if(state == State.ReturnToPosition)
            {
                // State should only be directly set to this for the end of a player's turn
                StartCoroutine(ReturnToPosition(State.EnemyShooting));
            }
            else if(state == State.ReturnOnBlank)
            {
                // State should only be directly set to this when the player shoots a blank shell at themselves
                StartCoroutine(ReturnToPosition(State.PlayerShooting));
            }
            else if(state == State.EnemyShooting)
            {
                // Can also be 0 when set to this state
                if(shellObjs.Count > 0)
                {
                    StartCoroutine(EvaluateTurn());
                }
                else
                {
                    StartCoroutine(ReturnToPosition(State.ItemsGiving));
                }
            }
            else if(state == State.ItemsGiving)
            {
                // No items or animations for this exist yet
                round++;
                state = State.Start;
            }
        }
    }

    private IEnumerator ReturnToPosition(State toState)
    {
        float timer = 0f;
        yield return new WaitForSeconds(1f);
        shotgunGrab.enabled = false;
        shotgunGrab.gameObject.GetComponent<Highlight>().OnFirstHoverExited(null);
        shotgunGrab.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        shotgunGrab.gameObject.GetComponent<BoxCollider>().enabled = false;
        sgBodyCol.enabled = false;
        sgPumpCol.enabled = false;
        while(timer < 2f)
        {
            shotgun.transform.position = Vector3.Lerp(shotgun.transform.position, ShotgunDefaultPos.position, timer/2f);
            shotgun.transform.rotation = Quaternion.Lerp(shotgun.transform.rotation, ShotgunDefaultPos.rotation, timer/2f);
            timer += Time.deltaTime;
            yield return null;
        }
        shotgun.transform.position = ShotgunDefaultPos.position;
        shotgun.transform.rotation = ShotgunDefaultPos.rotation;
        state = toState;
    }

    public void EvalAgain()
    {
        // Called when the dealer shoots a blank at himself
        StartCoroutine(EvaluateTurn());
    }

    private IEnumerator EvaluateTurn()
    {
        blankShellsLeft = 0;
        liveShellsLeft = 0;
        // Count shells
        foreach(GameObject shell in shellObjs)
        {
            if(shell.GetComponent<Shell>().blank)
            {
                blankShellsLeft++;
            }
            else
            {
                liveShellsLeft++;
            }
        }
        if(enemyItems.Count > 0)
        {
            // Use items and re-evaluate
            yield return StartCoroutine(EvaluateTurn());
            yield break;
        }
        else if(shellObjs.Count > 0) // Can also be called at 0 shells on a re-evaluation
        {
            yield return new WaitForSeconds(2f);
            if(doesHeKnow) // Will be true if dealer used magnifying glass item
            {
                if(shellObjs[0].GetComponent<Shell>().blank)
                {
                    yield return StartCoroutine(enemy.ShootSelf());
                }
                else
                {
                    yield return StartCoroutine(enemy.ShootPlayer());
                }
            }
            else
            {
                // Calculate chances of live shell
                int totalShells = blankShellsLeft + liveShellsLeft;
                float shotChance = (float)liveShellsLeft / totalShells;
                if(Random.Range(0f, 1f) <= shotChance)
                {
                    yield return StartCoroutine(enemy.ShootPlayer());
                }
                else
                {
                    yield return StartCoroutine(enemy.ShootSelf());
                }
            }
        }
        else
        {
            yield return StartCoroutine(ReturnToPosition(State.ItemsGiving));
        }
    }
}
