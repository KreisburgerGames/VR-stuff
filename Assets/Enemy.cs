using System.Collections;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

public class Enemy : MonoBehaviour
{
    private Game game;
    private Animator animator;
    public GameObject shellObj;
    public Transform shellEjectPoint;
    public GameObject shotgun, playerShotgun;

    void Start()
    {
        game = FindFirstObjectByType<Game>();
        animator = GetComponent<Animator>();
        shotgun.SetActive(false);
    }

    public void EjectShell()
    {
        // Ejects the shell just like in Pump.cs
        shellObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        shellObj.GetComponent<Rigidbody>().isKinematic = false;
        shellObj.GetComponent<Rigidbody>().useGravity = true;
        shellObj.GetComponent<BoxCollider>().enabled = true;
        shellObj.transform.parent = null;
        shellObj.GetComponent<Rigidbody>().AddForce(shellEjectPoint.right * UnityEngine.Random.Range(2f, 4f), ForceMode.Impulse);
        shellObj.GetComponent<Rigidbody>().AddTorque(shellEjectPoint.up * UnityEngine.Random.Range(-20f, 20f), ForceMode.Impulse);
        game.shellObjs.RemoveAt(0);
    }

    public IEnumerator ShootSelf()
    {
        print("shooting self");
        // Grab reference to the current shell and put it in the dealer's animated shotgun
        shellObj = game.shellObjs[0];
        shellObj.SetActive(true);
        // Swap which shotguns are visible to start the dealer's animation
        shotgun.SetActive(true);
        playerShotgun.SetActive(false);
        shellObj.transform.parent = shellEjectPoint;
        shellObj.transform.localPosition = Vector3.zero;
        shellObj.transform.localEulerAngles = Vector3.zero;
        yield return new WaitForSeconds(1f);
        animator.Play("e_grab_shotgun");
        yield return null; // Wait one frame to allow the animator to change states
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null; // Waits for animation to finish
        yield return new WaitForSeconds(0.5f);
        animator.Play("e_aim_self");
        yield return null;
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
        yield return new WaitForSeconds(0.5f);
        if(shellObj.GetComponent<Shell>().blank)
        {
            animator.Play("e_self_blank");
            yield return null;
            while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
            // Swap visibility of shotguns
            shotgun.SetActive(false);
            playerShotgun.SetActive(true);
            game.EvalAgain();
        }
        else
        {
            animator.Play("e_self_live");
            yield return null;
            while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
            shotgun.SetActive(false);
            playerShotgun.SetActive(true);
            game.enemyLives--;
            if(game.enemyLives == 0)
                game.state = Game.State.ReturnThenNextStage;
            else
                game.state = Game.State.PlayerShooting;
        }
    }

    public void Reveal()
    {
        // Called as animation event after trigger is pulled by the dealer
        shellObj.GetComponent<Shell>().RevealShell();
    }

    public IEnumerator ShootPlayer()
    {
        // Works the exact same way as ShootSelf()
        print("shooting player");
        shellObj = game.shellObjs[0];
        shellObj.SetActive(true);
        shotgun.SetActive(true);
        playerShotgun.SetActive(false);
        shellObj.transform.parent = shellEjectPoint;
        shellObj.transform.localPosition = Vector3.zero;
        shellObj.transform.localEulerAngles = Vector3.zero;
        yield return new WaitForSeconds(1f);
        animator.Play("e_grab_shotgun");
        yield return null;
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) {print(animator.GetCurrentAnimatorStateInfo(0).normalizedTime); yield return null;}
        yield return new WaitForSeconds(0.5f);
        animator.Play("e_aim_player");
        yield return null;
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
        yield return new WaitForSeconds(0.5f);
        if(shellObj.GetComponent<Shell>().blank)
        {
            animator.Play("e_player_blank");
            yield return null;
            while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
            shotgun.SetActive(false);
            playerShotgun.SetActive(true);
            game.state = Game.State.PlayerShooting;
        }
        else
        {
            animator.Play("e_player_live");
            yield return null;
            while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;
            shotgun.SetActive(false);
            playerShotgun.SetActive(true);
            game.state = Game.State.PlayerShooting;
        }
    }

    public void PlayerHit()
    {
        game.playerLives -= 1;
        if (game.playerLives == 0) UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
