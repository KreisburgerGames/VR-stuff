using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ItemSlot : MonoBehaviour
{
    public void OnSelect(SelectEnterEventArgs args)
    {
        args.interactableObject.transform.gameObject.GetComponent<Item>().Check();
    }
}
