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
    private bool isClimbingOverWall;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void HandleFixedUpdate()
    {
        if (isClimbingOverWall) return;

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

        // 마우스가 위쪽에 있고 플레이어 근처에 있을 때
        if (mouseWorldPos.y > transform.position.y + 0.5f &&
            Mathf.Abs(mouseWorldPos.x - transform.position.x) < 1f)
        {
            // 벽 끝에 도달했는지 체크
            if (IsAtWallTop())
            {
                StartClimbOver();
            }
            else
            {
                ClimbWall();
            }
        }
        else if (mouseWorldPos.y < transform.position.y - 0.5f)
        {
            FallOffWall();
        }
        else
        {
            // 마우스가 중간에 있으면 벽에 붙어있기
            controller.Rb.velocity = Vector2.zero;
        }
    }

    private bool IsAtWallTop()
    {
        float wallTopY = GetWallTopY();
        return wallTopY == float.MaxValue || transform.position.y >= wallTopY - 0.2;
    }

    private float GetWallTopY()
    {
        Collider2D col = GetWallCollider();
        return col != null ? col.bounds.max.y : float.MaxValue;
    }

    private Collider2D GetWallCollider()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.right * dir,
            0.8f,
            LayerMask.GetMask("Wall")
        );
        return hit.collider;
    }

    private void StartClimbOver()
    {
        // 벽 넘기 중일때는 무시
        if (isClimbingOverWall)
        {
            return;
        }

        isClimbingOverWall = true;
        isClimbing = false;
        isOnWall = false;

        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Climbing);

        StartCoroutine(SimpleClimbOver());
    }

    private IEnumerator SimpleClimbOver()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1;

        // DOTween 이동 중 물리 충돌 방지
        controller.Rb.velocity = Vector2.zero;
        controller.Rb.gravityScale = 0f;

        Vector3 startPos = transform.position;
        Vector3 target = new Vector3(
            startPos.x + dir * 1f,  // 벽 위로 살짝 이동 (겹치지 않게)
            startPos.y + 2f,        // 벽 높이만큼 위로 이동
            startPos.z
        );

        float jumpPower = 1.2f; // 포물선 정점 높이
        float duration = 0.6f;

        // 포물선 점프
        transform
            .DOJump(target, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad)
            .OnStart(() =>
            {
                controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
            })
            .OnComplete(() =>
            {
                controller.Rb.gravityScale = 3f;
                controller.SetCanMove(true);

                ResetWallState();
            });

        yield return new WaitForSeconds(duration);
    }


    public void StickToWall()
    {
        isOnWall = true;
        isClimbing = false;
        isWallJumping = false;
        isClimbingOverWall = false;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Climbing);
        controller.Rb.gravityScale = 0f;
        controller.Rb.velocity = Vector2.zero;
    }

    public void JumpFromWall()
    {
        if (!isOnWall || isClimbingOverWall) return;

        isWallJumping = true;
        isOnWall = false;
        isClimbingOverWall = false;

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
        if (isClimbingOverWall)
        {
            return;
        }

        isOnWall = false;
        isClimbing = false;
        isFallingDelay = true;
        isClimbingOverWall = false;
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
        isClimbing = true;
        controller.Rb.velocity = new Vector2(0f, controller.data.climbSpeed);
        controller.Rb.gravityScale = 0f;
    }

    private IEnumerator EnableMoveAfterDelay()
    {
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Landing);
        yield return new WaitForSeconds(controller.data.moveDelayAfterFall);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
        controller.SetCanMove(true);
        isFallingDelay = false;
    }

    public void ResetWallState()
    {
        controller.Rb.gravityScale = 3f;
        isOnWall = false;
        isClimbing = false;
        isWallJumping = false;
        isClimbingOverWall = false;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
        controller.SetCanMove(true);
    }

    // 기존 메서드들
    public void HandleUpdate() { }
    public bool IsOnWall() => isOnWall;
    public bool IsClimbing() => isClimbing;
    public bool IsFallingDelay() => isFallingDelay;
    public bool IsWallJumping() => isWallJumping;
    public bool IsClimbingOverWall() => isClimbingOverWall;

    #if UNITY_EDITOR
private void OnDrawGizmos()
{
    if (!Application.isPlaying) return;

    // ─────────────── 벽 감지 Ray ───────────────
    float dir = transform.localScale.x > 0 ? 1 : -1;
    Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
    float rayDistance = 0.6f;

    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dir, rayDistance, LayerMask.GetMask("Wall"));

    // Ray 색상
    Gizmos.color = hit ? Color.green : Color.red;
    Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * dir * rayDistance);

    // Ray 끝점
    Gizmos.DrawSphere(rayOrigin + Vector2.right * dir * rayDistance, 0.03f);

    // ─────────────── 벽 상단 표시 ───────────────
    if (hit.collider != null)
    {
        float wallTopY = hit.collider.bounds.max.y;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(hit.collider.bounds.min.x, wallTopY, 0f),
            new Vector3(hit.collider.bounds.max.x, wallTopY, 0f)
        );

        // 현재 플레이어 y 위치 비교 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(hit.collider.bounds.center.x - 0.2f, transform.position.y, 0f),
            new Vector3(hit.collider.bounds.center.x + 0.2f, transform.position.y, 0f)
        );

        // 텍스트처럼 구분되도록 (Scene뷰 Gizmo 아이콘)
        UnityEditor.Handles.Label(
            hit.collider.bounds.center + Vector3.up * 0.2f,
            $"WallTopY: {wallTopY:F2}\nPlayerY: {transform.position.y:F2}"
        );
    }
}
#endif

}
