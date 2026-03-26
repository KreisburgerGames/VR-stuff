using System.Collections;
using UnityEngine;

public class HandsawItem : MonoBehaviour
{
    private Coroutine openCloseCoroutine;
    private Item item;
    private bool isOpened;
    public GameObject blade;
    public float openCloseTime = .5f;
    
    void Start()
    {
        item = GetComponent<Item>();
    }

    public void OpenClose()
    {
        if(!item.canUse) return;
        if(openCloseCoroutine != null) return; // Prevents starting the coroutine multiple times if the player spams the use button
        openCloseCoroutine = StartCoroutine(OpenCloseCoroutine());
    }

    private IEnumerator OpenCloseCoroutine()
    {
        float timer = 0f;
        if(!isOpened)
        {
            while (timer < openCloseTime)
            {
                blade.transform.localEulerAngles = Vector3.Lerp(new Vector3(90, 0, 0f), new Vector3(-90, 0, 1f), timer / openCloseTime);
                timer += Time.deltaTime;
                yield return null;
            }
            blade.transform.localEulerAngles = new Vector3(-90, 0, 1f);
            isOpened = true;
        }
        else
        {
            while (timer < openCloseTime)
            {
                blade.transform.localEulerAngles = Vector3.Lerp(new Vector3(-90, 0, 1f), new Vector3(90, 0, 0f), timer / openCloseTime);
                timer += Time.deltaTime;
                yield return null;
            }
            blade.transform.localEulerAngles = new Vector3(90, 0, 0f);
            isOpened = false;
        }
        openCloseCoroutine = null;
    }
}
