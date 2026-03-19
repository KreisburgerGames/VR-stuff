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
    private Item item;
    private Pump pump;
    private Game game;

    void Awake()
    {
        pump = FindFirstObjectByType<Pump>();
        item = GetComponent<Item>();
        game = FindFirstObjectByType<Game>();
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.tag);
        if(other.gameObject.tag == "Table")
        {
            print(rb.linearVelocity.magnitude);
        }
        if(item.canUse && !used && other.gameObject.tag == "Table" && rb.linearVelocity.magnitude >= minimumBreakMagnitute)
        {
            meshFilter.mesh = brokenMesh;
            used = true;
            pump.UnlockForCheck();
            outside.SetActive(false);
            game.playerItems.Remove(gameObject);
        }
    }
}
