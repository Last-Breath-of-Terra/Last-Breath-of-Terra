using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PlayerWallClimb : MonoBehaviour
{
    private PlayerController controller;

    private bool isOnWall;
    private bool isClimbing;
    private bool isWallJumping;
    private bool isFallingDelay;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void HandleUpdate()
    {
        if (!isOnWall || !controller.canMove) return;

        if (!controller.Movement.IsHoldingClick)
        {
            if (!isFallingDelay && !isWallJumping)
            {
                FallOffWall();
            }

            return;
        }

        Vector2 mouseWorldPos = GameManager.Instance._ui.GetMouseWorldPosition();

        if (mouseWorldPos.y > transform.position.y + 0.5f && Mathf.Abs(mouseWorldPos.x - transform.position.x) < 1f)
        {
            ClimbWall();
        }
        else if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
        }
        else if (mouseWorldPos.y < transform.position.y - 0.5f)
        {
            FallOffWall();
        }
    }

    public void HandleFixedUpdate() {}

    public bool IsOnWall() => isOnWall;
    public bool IsClimbing() => isClimbing;

    public void StickToWall()
    {
        isOnWall = true;
        isClimbing = false;
        isWallJumping = false;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Climbing);
        controller.Rb.gravityScale = 0f;
        controller.Rb.velocity = Vector2.zero;
    }

    public void JumpFromWall()
    {
        if (!isOnWall) return;

        isWallJumping = true;
        isOnWall = false;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        float jumpDir = worldPos.x > transform.position.x ? 1f : -1f;

        controller.Rb.gravityScale = 3f;
        controller.Rb.velocity = new Vector2(jumpDir * controller.data.walljumpForce, controller.data.jumpForce);

        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Jump);
        transform.localScale = new Vector3(jumpDir * Mathf.Abs(controller.OriginalScale.x), controller.OriginalScale.y, controller.OriginalScale.z);
    }

    public void FallOffWall()
    {
        isOnWall = false;
        isClimbing = false;
        isFallingDelay = true;
        controller.SetCanMove(false);

        float backForce = 2f;
        float dir = transform.localScale.x > 0 ? -1f : 1f;

        controller.Rb.gravityScale = 3f;
        controller.Rb.velocity = new Vector2(dir * backForce, -2f);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Fall);

        controller.StartCoroutine(EnableMoveAfterDelay());
    }

    private void ClimbWall()
    {
        if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
            return;
        }
        isClimbing = true;
        controller.Rb.velocity = new Vector2(0f, controller.data.climbSpeed);
        controller.Rb.gravityScale = 0f;
    }

    private IEnumerator EnableMoveAfterDelay()
    {
        yield return new WaitForSeconds(controller.data.moveDelayAfterFall);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Landing);
        controller.SetCanMove(true);
        isFallingDelay = false;
    }

    private void AutoMoveAfterWallTop()
    {
        if (!isClimbing && !IsAtWallTop()) return;

        isClimbing = false;
        isOnWall = false;
        controller.Rb.gravityScale = 0f;

        float upward = 2f;
        float forward = 0.5f;
        float dir = transform.localScale.x > 0 ? 1 : -1;

        Vector3 target = transform.position + new Vector3(forward * dir, upward, 0f);
        transform.DOMove(target, 0.5f).OnComplete(() => ResetWallState());
    }

    private bool IsAtWallTop()
    {
        float wallTopY = GetWallTopY();
        return wallTopY == float.MaxValue || transform.position.y >= wallTopY;
    }

    private float GetWallTopY()
    {
        Collider2D col = GetWallCollider();
        return col != null ? col.bounds.max.y : float.MaxValue;
    }

    private Collider2D GetWallCollider()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * dir, 0.5f, LayerMask.GetMask("Wall"));
        return hit.collider;
    }

    private void ResetWallState()
    {
        controller.Rb.gravityScale = 3f;
        controller.Rb.velocity = Vector2.zero;
        isOnWall = false;
        isClimbing = false;
        isWallJumping = false;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
    }

    public bool IsFallingDelay() => isFallingDelay;
    public bool IsWallJumping() => isWallJumping;
}