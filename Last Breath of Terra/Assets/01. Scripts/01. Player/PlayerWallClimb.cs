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

    private float wallTopDetectionDistance = 18f;
    private float climbOverSpeed = 10f;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void HandleUpdate()
    {        
        // 벽 넘기 중일 때는 아무것도 하지 않음
        if (isClimbingOverWall) 
        {
            return;
        }
        
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
            if (IsNearWallTop())
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

    private bool IsNearWallTop()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1;
        
        // 현재 위치에서 주황색 벽(Wall 레이어) 가져오기
        RaycastHit2D currentWallHit = Physics2D.Raycast(
            transform.position, 
            Vector2.right * dir, 
            0.6f, 
            LayerMask.GetMask("Wall")
        );
        
        if (currentWallHit.collider == null) 
        {
            return false;
        }
        
        // 벽의 실제 최대 높이와 최소 높이 구하기
        float wallMaxY = currentWallHit.collider.bounds.max.y;
        float wallMinY = currentWallHit.collider.bounds.min.y;
        float wallHeight = wallMaxY - wallMinY;
        float playerY = transform.position.y;
        float distanceToTop = wallMaxY - playerY;
        
        // 벽 끝에서 wallTopDetectionDistance 이내에 있으면 벽 끝으로 판단
        float detectionThreshold = wallTopDetectionDistance; // 고정값 사용
        bool nearTop = distanceToTop <= detectionThreshold;
        
        return nearTop;
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
        isOnWall = false; // 벽에서 떨어져도 OnCollisionExit2D 무시되도록 함
        
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Climbing);
        
        StartCoroutine(SimpleClimbOver());
    }

    private IEnumerator SimpleClimbOver()
    {   
        float dir = transform.localScale.x > 0 ? 1 : -1;
        Debug.Log($"이동 방향: {dir} (왼쪽=-1, 오른쪽=1)");
        
        controller.Rb.gravityScale = 0f;
        
        // 1단계: 살짝 위로 이동
        Debug.Log("1단계: 위로 이동 시작");
        float upTime = 0.3f;
        float currentUpTime = 0f;
        
        while (currentUpTime < upTime)
        {
            Vector2 newVelocity = new Vector2(0f, climbOverSpeed);
            controller.Rb.velocity = newVelocity;
            Debug.Log($"위로 이동 설정: {newVelocity}, 실제 velocity: {controller.Rb.velocity}, 위치: {transform.position}");
            
            currentUpTime += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("1단계 완료, 2단계: 앞으로 이동 시작");
        
        // 2단계: 앞으로 이동
        float forwardTime = 0.45f;
        float currentForwardTime = 0f;
        
        while (currentForwardTime < forwardTime)
        {
            float progress = currentForwardTime / forwardTime;
            float forwardSpeed = Mathf.Lerp(4f, 2f, progress);
            float upSpeed = Mathf.Lerp(1f, 0f, progress);
            
            Vector2 newVelocity = new Vector2(dir * forwardSpeed, upSpeed);
            controller.Rb.velocity = newVelocity;
            Debug.Log($"앞으로 이동 설정: {newVelocity}, 실제 velocity: {controller.Rb.velocity}");
            
            currentForwardTime += Time.deltaTime;
            yield return null;
        }
        
        // 벽 넘기 완료
        ResetWallState();
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
        yield return new WaitForSeconds(controller.data.moveDelayAfterFall);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Landing);
        controller.SetCanMove(true);
        isFallingDelay = false;
    }

    private void ResetWallState()
    {
        controller.Rb.gravityScale = 3f;
        isOnWall = false;
        isClimbing = false;
        isWallJumping = false;
        isClimbingOverWall = false;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
    }

    // 기존 메서드들
    public void HandleFixedUpdate() {}
    public bool IsOnWall() => isOnWall;
    public bool IsClimbing() => isClimbing;
    public bool IsFallingDelay() => isFallingDelay;
    public bool IsWallJumping() => isWallJumping;
    public bool IsClimbingOverWall() => isClimbingOverWall;
}

// using UnityEngine;
// using System.Collections;
// using DG.Tweening;
// using UnityEngine.InputSystem;

// public class PlayerWallClimb : MonoBehaviour
// {
//     private PlayerController controller;

//     private bool isOnWall;
//     private bool isClimbing;
//     private bool isWallJumping;
//     private bool isFallingDelay;


//     void Awake()
//     {
//         controller = GetComponent<PlayerController>();
//     }

//     public void HandleUpdate()
//     {
//         if (!isOnWall || !controller.canMove) return;

//         if (!controller.Movement.IsHoldingClick)
//         {
//             if (!isFallingDelay && !isWallJumping)
//             {
//                 FallOffWall();
//             }

//             return;
//         }

//         Vector2 mouseWorldPos = GameManager.Instance._ui.GetMouseWorldPosition();

//         if (mouseWorldPos.y > transform.position.y + 0.5f && Mathf.Abs(mouseWorldPos.x - transform.position.x) < 1f)
//         {
//             ClimbWall();
//         }
//         else if (IsAtWallTop())
//         {
//             AutoMoveAfterWallTop();
//         }
//         else if (mouseWorldPos.y < transform.position.y - 0.5f)
//         {
//             FallOffWall();
//         }
//     }

//     public void HandleFixedUpdate() {}

//     public bool IsOnWall() => isOnWall;
//     public bool IsClimbing() => isClimbing;

//     public void StickToWall()
//     {
//         isOnWall = true;
//         isClimbing = false;
//         isWallJumping = false;
//         controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Climbing);
//         controller.Rb.gravityScale = 0f;
//         controller.Rb.velocity = Vector2.zero;
//     }

//     public void JumpFromWall()
//     {
//         if (!isOnWall) return;

//         isWallJumping = true;
//         isOnWall = false;

//         Vector2 mousePos = Mouse.current.position.ReadValue();
//         Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
//         float jumpDir = worldPos.x > transform.position.x ? 1f : -1f;

//         controller.Rb.gravityScale = 3f;
//         controller.Rb.velocity = new Vector2(jumpDir * controller.data.walljumpForce, controller.data.jumpForce);

//         controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Jump);
//         transform.localScale = new Vector3(jumpDir * Mathf.Abs(controller.OriginalScale.x), controller.OriginalScale.y, controller.OriginalScale.z);
//     }

//     public void FallOffWall()
//     {
//         isOnWall = false;
//         isClimbing = false;
//         isFallingDelay = true;
//         controller.SetCanMove(false);

//         float backForce = 2f;
//         float dir = transform.localScale.x > 0 ? -1f : 1f;

//         controller.Rb.gravityScale = 3f;
//         controller.Rb.velocity = new Vector2(dir * backForce, -2f);
//         controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Fall);

//         controller.StartCoroutine(EnableMoveAfterDelay());
//     }

//     private void ClimbWall()
//     {
//         if (IsAtWallTop())
//         {
//             AutoMoveAfterWallTop();
//             return;
//         }
//         isClimbing = true;
//         controller.Rb.velocity = new Vector2(0f, controller.data.climbSpeed);
//         controller.Rb.gravityScale = 0f;
//     }

//     private IEnumerator EnableMoveAfterDelay()
//     {
//         yield return new WaitForSeconds(controller.data.moveDelayAfterFall);
//         controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Landing);
//         controller.SetCanMove(true);
//         isFallingDelay = false;
//     }

//     private void AutoMoveAfterWallTop()
//     {
//         if (!isClimbing && !IsAtWallTop()) return;

//         isClimbing = false;
//         isOnWall = false;
//         controller.Rb.gravityScale = 0f;

//         float upward = 2f;
//         float forward = 0.5f;
//         float dir = transform.localScale.x > 0 ? 1 : -1;

//         Vector3 target = transform.position + new Vector3(forward * dir, upward, 0f);
//         transform.DOMove(target, 0.5f).OnComplete(() => ResetWallState());
//     }

//     private bool IsAtWallTop()
//     {
//         float wallTopY = GetWallTopY();
//         return wallTopY == float.MaxValue || transform.position.y >= wallTopY;
//     }

//     private float GetWallTopY()
//     {
//         Collider2D col = GetWallCollider();
//         return col != null ? col.bounds.max.y : float.MaxValue;
//     }

//     private Collider2D GetWallCollider()
//     {
//         float dir = transform.localScale.x > 0 ? 1 : -1;
//         RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * dir, 0.5f, LayerMask.GetMask("Wall"));
//         return hit.collider;
//     }

//     private void ResetWallState()
//     {
//         controller.Rb.gravityScale = 3f;
//         controller.Rb.velocity = Vector2.zero;
//         isOnWall = false;
//         isClimbing = false;
//         isWallJumping = false;
//         controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
//     }

//     public bool IsFallingDelay() => isFallingDelay;
//     public bool IsWallJumping() => isWallJumping;
// }