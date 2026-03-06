using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shell : MonoBehaviour
{
    private BoxCollider boxCol;
    public float waitPeriod = 0.2f;
    private bool done = false;

    private void Start() {
        boxCol = GetComponent<BoxCollider>();
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
