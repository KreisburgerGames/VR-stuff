using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimation : MonoBehaviour
{
    [SerializeField] private InputActionReference gripActionReference;
    [SerializeField] private InputActionReference triggerActionReference;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float gripValue = gripActionReference.action.ReadValue<float>();
        float triggerValue = triggerActionReference.action.ReadValue<float>();
        animator.SetFloat("Grip", gripValue);
        animator.SetFloat("Pinch", triggerValue);
    }
}
