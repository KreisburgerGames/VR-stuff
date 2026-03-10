using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shell : MonoBehaviour
{
    private BoxCollider boxCol;
    public float waitPeriod = 0.2f;
    private bool done = false;
    public Material blankMat;
    private Material current;
    public bool blank;

    private void Start() {
        boxCol = GetComponent<BoxCollider>();
        if (blank) GetComponent<MeshRenderer>().material = blankMat;
        current = GetComponent<MeshRenderer>().material;
    }

    public void Hide()
    {
        current.color = new Color(0, 0, 0);
    }

    public void RevealShell()
    {
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        float timer = 0f;
        while(timer < 1f)
        {
            timer += Time.deltaTime;
            current.color = Color.Lerp(current.color, Color.white, timer);
            yield return null;
        }
        current.color = Color.white;
    }

    private IEnumerator DisableIsTrigger()
    {
        yield return new WaitForSeconds(waitPeriod);
        boxCol.isTrigger = false;
    }

    void Update()
    {
        if(boxCol.enabled && !done)
        {
            StartCoroutine(DisableIsTrigger());
            done = true;
        }
    }
}
