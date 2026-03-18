using UnityEngine;
using UnityEngine.PlayerLoop;

public class MagnifyingGlassItem : MonoBehaviour
{
    public Rigidbody rb;
    public float minimumBreakMagnitute = 3f;
    public Mesh brokenMesh;
    public GameObject outside;
    public MeshFilter meshFilter;
    private bool used = false;
    private Pump pump;

    void Awake()
    {
        pump = FindFirstObjectByType<Pump>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(!used && collision.gameObject.tag == "Table" && rb.linearVelocity.magnitude >= minimumBreakMagnitute)
        {
            meshFilter.mesh = brokenMesh;
            used = true;
            pump.UnlockForCheck();
            outside.SetActive(false);
        }
    }
}
