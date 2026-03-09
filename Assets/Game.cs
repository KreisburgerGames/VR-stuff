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

    public enum State
    {
        Start,
        LoadingShells,
        PlayerShooting,
        EnemyShooting,
        ReturnToPosition,
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
        yield return new WaitForSeconds(3f);
        foreach(GameObject shell in shellObjs)
        {
            shell.transform.position = new Vector3(shell.transform.position.x, shell.transform.position.y - 5f, shell.transform.position.z);
            shell.GetComponent<Shell>().Hide();
        }
        GameObject[] s = (GameObject[])ArrayExtensionMethods.Shuffle(shellObjs.ToArray());
        shellObjs = s.ToList();
        pump.Chamber();
        state = State.PlayerShooting;
    }

    void Update()
    {
        if(state != oldState)
        {
            oldState = state;
            if(state == State.Start)
            {
                int shellGen = Random.Range(shellsMin + round, shellsMax + round);
                for(int i = 0; i < shellGen; i++)
                {
                    GameObject shell = Instantiate(shellPrefab, shellsPoint.position, Quaternion.identity);
                    if (Random.Range(0, 2) == 0) {shell.GetComponent<Shell>().blank = true; blankShellsLeft++;}
                    else liveShellsLeft++;
                    shell.transform.parent = shellsPoint;
                    shell.transform.position = new Vector3(shell.transform.position.x + shellsOffset * i, shell.transform.position.y, shell.transform.position.z);
                    shellObjs.Add(shell);
                }
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
            }
            if(state == State.LoadingShells)
            {
                
            }
            else if(state == State.PlayerShooting)
            {
                if(shellObjs.Count == 0)
                {
                    StartCoroutine(ReturnToPosition(State.ItemsGiving));
                    return;
                }
                shotgunGrab.enabled = true;
                pump.canShoot = true;
            }
            else if(state == State.ReturnToPosition)
            {
                StartCoroutine(ReturnToPosition(State.EnemyShooting));
            }
            else if(state == State.EnemyShooting)
            {
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
        while(timer < 2f)
        {
            shotgun.transform.position = Vector3.Lerp(shotgun.transform.position, ShotgunDefaultPos.position, timer);
            shotgun.transform.rotation = Quaternion.Lerp(shotgun.transform.rotation, ShotgunDefaultPos.rotation, timer);
            timer += Time.deltaTime;
            yield return null;
        }
        state = toState;
    }

    private IEnumerator EvaluateTurn()
    {
        if(enemyItems.Count > 0)
        {
            
            StartCoroutine(EvaluateTurn());
        }
        else
        {
            yield return new WaitForSeconds(2f);
            if(doesHeKnow)
            {
                if(shellObjs[0].GetComponent<Shell>().blank)
                {
                    // Shoot self
                }
                else
                {
                   // Shoot player
                }
            }
            else
            {
                int totalShells = blankShellsLeft + liveShellsLeft;
                float shotChance = (float)liveShellsLeft / totalShells;
                if(Random.Range(0f, 1f) <= shotChance)
                {
                    // Shoot player
                }
                else
                {
                    // Shoot self
                }
            }
            if(shellObjs.Count > 0)
            {
                state = State.PlayerShooting;
            }
            else
            {
                state = State.ItemsGiving;
            }
        }
    }
}
