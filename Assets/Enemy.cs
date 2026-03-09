using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;

public class Enemy : MonoBehaviour
{
    private Game game;
    private Animator animator;

    void Start()
    {
        game = FindFirstObjectByType<Game>();
        animator = GetComponent<Animator>();
    }

    public void Self()
    {
        
    }

    public void Player()
    {
        
    }
}
