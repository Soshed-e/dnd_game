// Animation-related script
using UnityEngine;
using UnityEngine.AI;

public class AnimationController : MonoBehaviour
{
    public Animator animator;

    private bool isRunning;
    private bool isStunned = false;

    public void SetRunning(bool running)
    {
        isRunning = running;
        animator.SetBool("IsRunning", isRunning);
    }

    public void TriggerAttack(int attackType)
    {
        if (attackType == 1)
            animator.SetTrigger("Attack1");
        else if (attackType == 2)
            animator.SetTrigger("Attack2");
    }

    public void TriggerPickup()
    {
        animator.SetTrigger("PickUp");
    }

    public void TriggerHit()
    {
        animator.SetTrigger("Hit");
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Die");
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }
}
