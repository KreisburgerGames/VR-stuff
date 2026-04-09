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
    public int blankShellsLeft, liveShellsLeft;
    public bool doesHeKnow = false;
    public Enemy enemy;
    public BoxCollider sgBodyCol, sgPumpCol; // Both attachment transform colliders on the shotgun
    public List<GameObject> itemPrefabs = new List<GameObject>();
    public List<GameObject> playerItems {get; private set;} = new List<GameObject>(); public List<GameObject> enemyItems {get; private set;} = new List<GameObject>();
    public GameObject itemCrate;
    public Transform spawnPoint;
    public int playerLives, enemyLives = 2;

    public enum State
    {
        Start,
        LoadingShells,
        PlayerShooting,
        EnemyShooting,
        ReturnToPosition,
        ReturnOnBlank,
        ItemsGiving,
        NextStage,
        ReturnThenNextStage,
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

    public void EnableItems()
    {
        foreach(GameObject item in playerItems)
        {
            item.GetComponent<Item>().canUse = true;
        }
    }

    public void DisableItems()
    {
        foreach(GameObject item in playerItems)
        {
            item.GetComponent<Item>().canUse = false;
        }
    }

    void Update()
    {
        if(state != oldState) // Only runs ONCE per state change
        {
            oldState = state;
            if(state == State.Start)
            {
                // Choose between 4 and 8 shells but slightly influenced off what round it is
                int min = 0, max = 0;
                int floored = Mathf.FloorToInt(round / 2f);
                if(stage == 1)
                {
                    min = shellsMin; max = shellsMax + floored;
                }
                else
                {
                    min = shellsMin + floored; max = shellsMax + floored;
                    min = Mathf.Clamp(min, 3, 4);
                    max = Mathf.Clamp(max, 4, 8);
                }
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
                EnableItems();
            }
            else if(state == State.ReturnThenNextStage)
            {
                StartCoroutine(ReturnToPosition(State.NextStage));
            }
            else if(state == State.NextStage)
            {
                for(int i = 0; i < shellObjs.Count; i++)
                {
                    Destroy(shellObjs[0]);
                    shellObjs.RemoveAt(0);
                }
                stage++;
                if(stage == 2) {enemyLives = 4; playerLives = 4;}
                else {enemyLives = 6; playerLives = 6;}
                state = State.ItemsGiving;
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
                StartCoroutine(HandItems());
            }
        }
    }

    private IEnumerator HandItems()
    {
        // Items only on stage 2 and 3
        int itemsToGive = 0;
        if(stage == 2 || true) itemsToGive = 2;
        else if (stage == 3)
        {
            /* 
            Stage 3 begins - 4 items
            Stage 3 when shells run out - 2 more items
            */
            if(round == 1) itemsToGive = 4; else itemsToGive = 2; // Should also eventually add it where Stage 3 round 1 your old items are cleared
        }
        if(itemsToGive > 0)
        {
            itemCrate.SetActive(true);
            for(int i = 0; i < itemsToGive; i++)
            {
                // Spawn item, initialize, and wait until the player grabs the item
                GameObject itemToGive = itemPrefabs[UnityEngine.Random.Range(0, itemPrefabs.Count)];
                GameObject item = Instantiate(itemToGive, spawnPoint, false);
                ItemBox boxRef = itemCrate.GetComponent<ItemBox>();
                boxRef.SetNext(item);
                while(boxRef.nextObject != null) yield return null;
            }
            yield return new WaitForSeconds(1f);
            itemCrate.SetActive(false);
        }
        round++;
        state = State.Start;
    }

    private IEnumerator ReturnToPosition(State toState)
    {
        if(pump.barrelCut) yield return StartCoroutine(pump.ResetBarrel());
        if(pump.xRGrab.attachTransform != pump.primaryGrabPos) pump.xRGrab.attachTransform = pump.primaryGrabPos;
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
