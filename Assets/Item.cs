using UnityEngine;

public class Item : MonoBehaviour
{
    bool colliderEnabled;
    [SerializeField] private Collider physicalCollider;

    public void Check()
    {
        if(!colliderEnabled) colliderEnabled = true; else return;
        physicalCollider.enabled = true;
    }
}
