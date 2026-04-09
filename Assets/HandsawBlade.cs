using UnityEngine;

public class HandsawBlade : MonoBehaviour
{
    public HandsawItem handsawItem;
    public Item item;
    private Vector3 lastPosition;
    public float minimumCutMagnitude = 0.15f;
    private float velocity;

    void Update()
    {
        velocity = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("CutPoint") && item.canUse && handsawItem.isOpened && !handsawItem.used && velocity >= minimumCutMagnitude) // Checks if the blade is moving fast enough to cut, and if the item can be used and is opened, and if it hasn't already been used
        {
            FindFirstObjectByType<Pump>().CutBarrel();
            item.canUse = false;
            handsawItem.used = true;
        }
    }
}
