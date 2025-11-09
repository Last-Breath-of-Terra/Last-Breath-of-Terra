using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationHandler : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Landing,
        Climbing,
        Knockdown,
        Knockback,
        Activating,
        MoveToPortal
    }

    private Animator animator;
    private AnimationState currentState = AnimationState.Idle;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeState(AnimationState newState)
    {
        if (currentState == newState) return;

        ResetAllTriggers();
        currentState = newState;

        switch (newState)
        {
            case AnimationState.Idle:
            case AnimationState.Walk:
            case AnimationState.Run:
                break;
            case AnimationState.Jump:
                animator.SetBool("isJump", true);
                break;
            case AnimationState.Fall:
                animator.SetBool("isFalling", true);
                break;
            case AnimationState.Landing:
                animator.SetTrigger("Landing");
                break;
            case AnimationState.Climbing:
                animator.SetBool("isClimbing", true);
                break;
            case AnimationState.Knockdown:
                animator.SetBool("isKnockdown", true);
                break;
            case AnimationState.Knockback:
                animator.SetBool("isKnockback", true);
                break;
            case AnimationState.Activating:
                animator.SetBool("isActivating", true);
                break;
            case AnimationState.MoveToPortal:
                animator.SetBool("MoveToPortal", true);
                break;
        }
    }

    private void ResetAllTriggers()
    {
        animator.SetBool("isJump", false);
        animator.ResetTrigger("Landing");
        animator.SetBool("isFalling", false);
        animator.SetBool("isClimbing", false);
        animator.SetBool("isKnockdown", false);
        animator.SetBool("isKnockback", false);
        animator.SetBool("isActivating", false);
        animator.SetBool("MoveToPortal", false);
    }

    public void UpdateSpeedParameter(float speed)
    {
        animator.SetFloat("currentSpeed", speed);
    }
}