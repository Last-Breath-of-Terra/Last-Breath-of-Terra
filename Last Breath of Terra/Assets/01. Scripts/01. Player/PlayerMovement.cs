using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController controller;

    private Vector2 targetPosition;
    private float currentSpeed;
    private float moveAccelerationTimer;
    private float footstepTimer;
    private float fallStartY;
    private readonly float footstepInterval = 0.5f;
    [SerializeField] private float speedChangeRate = 1f;
    [SerializeField] private float slideTurningFactor = 0.3f;
    [SerializeField] private float slideDeceleration = 0.97f;
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;
    private PhysicsMaterial2D originalMaterial;
    private float originalDrag;
    private bool isSliding = false;
    private bool _isJumping;
    private bool _isSignificantFall;
    private bool isSlowed;
    private bool isHoldingClick;
    private Coroutine speedDebuffCoroutine;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        originalMaterial = controller.Rb.sharedMaterial;
        originalDrag = controller.Rb.drag;
    }

    public void HandleUpdate()
    {
        if (!controller.canMove) return;

        if (isHoldingClick)
        {
            UpdateTargetPosition();
        }
    }

    public void HandleFixedUpdate()
    {
        if (!controller.canMove) return;

        UpdateAcceleration();
        UpdateFallingState();
        UpdateFallingSpeed();

        if (isHoldingClick && !controller.WallClimb.IsClimbing() && !controller.WallClimb.IsFallingDelay())
        {
            Move();
            UpdateFootstepSound();
        }
    }

    private void UpdateFallingState()
    {
        if (!IsGrounded() && controller.Rb.velocity.y < -1f)
        {
            if (!controller.WallClimb.IsOnWall() && !controller.WallClimb.IsClimbing())
            {
                controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Fall);
            }

            if (fallStartY == 0f)
                fallStartY = transform.position.y;

            float fallHeight = fallStartY - transform.position.y;
            _isSignificantFall = fallHeight > 2f;
        }

        if (IsGrounded())
        {
            _isJumping = false;

            if (_isSignificantFall)
            {
                _isSignificantFall = false;
                controller.StartCoroutine(HandleLandingDelay());
            }

            fallStartY = 0f;
        }
    }

    private void UpdateTargetPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 rawTarget = Camera.main.ScreenToWorldPoint(mousePos);

        if (isSliding)
        {
            // 방향 전환 둔화
            targetPosition = Vector2.Lerp(targetPosition, rawTarget, slideTurningFactor);
        }
        else
        {
            targetPosition = rawTarget;
        }

        if (Vector2.Distance(targetPosition, transform.position) > 0.1f)
        {
            GameManager.Instance._ui.HandleClickLight(targetPosition);
        }
    }

    private void UpdateAcceleration()
    {
        if (!isHoldingClick) { moveAccelerationTimer = 0; currentSpeed = 0; return; }

        if (Vector2.Distance(targetPosition, transform.position) < 0.1f)
        {
            moveAccelerationTimer -= Time.deltaTime * 5f;
        }
        else
        {
            moveAccelerationTimer += Time.deltaTime;
        }
        moveAccelerationTimer = Mathf.Clamp(moveAccelerationTimer, 0f, controller.data.moveAccelerationTime);
        currentSpeed = Mathf.Lerp(0f, controller.data.maxSpeed, moveAccelerationTimer / controller.data.moveAccelerationTime);
        currentSpeed *= speedChangeRate;
    }

    private void UpdateFallingSpeed()
    {
        if (controller.Rb.velocity.y < 0)
        {
            float newFallSpeed = controller.Rb.velocity.y - controller.data.fallAccelerationTime * Time.fixedDeltaTime;
            controller.Rb.velocity = new Vector2(controller.Rb.velocity.x, Mathf.Max(newFallSpeed, -controller.data.maxFallSpeed));
        }
    }

    private void Move()
    {
        float dir = Mathf.Sign(targetPosition.x - transform.position.x);
        float distanceX = Mathf.Abs(targetPosition.x - transform.position.x);

        if (distanceX > 0.5f)
        {
            transform.localScale = new Vector3(dir * Mathf.Abs(controller.OriginalScale.x), controller.OriginalScale.y, controller.OriginalScale.z);
            controller.Rb.velocity = new Vector2(dir * currentSpeed, controller.Rb.velocity.y);
        }
        else
        {
            if (isSliding)
            {
                float preservedDir = Mathf.Sign(controller.Rb.velocity.x);

                // 속도 낮아지면 멈춤 처리
                if (Mathf.Abs(controller.Rb.velocity.x) < 0.2f)
                {
                    controller.Rb.velocity = new Vector2(0f, controller.Rb.velocity.y);
                    ExitSliding();
                    return;
                }

                // 감속 적용
                controller.Rb.velocity = new Vector2(
                    preservedDir * controller.Rb.velocity.magnitude * slideDeceleration,
                    controller.Rb.velocity.y
                );
            }
            else
            {
                if (currentSpeed > 0.2f)
                    currentSpeed *= 0.8f;
                else
                    controller.Rb.velocity = new Vector2(0, controller.Rb.velocity.y);
            }
        }
    }

    private void UpdateFootstepSound()
    {
        if (!IsGrounded() || controller.Rb.velocity.magnitude < 0.1f) return;

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.Instance.PlayRandomPlayer(controller.AudioHandler.GetFootstepClipPrefix(), 0);
        }
    }

    public void StopMoving()
    {
        isHoldingClick = false;
        if (isSliding)
        {
            return;
        }

        controller.Rb.velocity = new Vector2(0, controller.Rb.velocity.y);
    }

    public void TryJump()
    {
        if (!IsGrounded()) return;

        _isJumping = true;
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Jump);
        fallStartY = transform.position.y;

        controller.Rb.velocity = new Vector2(controller.Rb.velocity.x, controller.data.jumpForce);

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        GameManager.Instance._ui.HandleJumpLight(worldPos);
    }

    public void TryAttack()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<Obstacle>(out var obstacle))
            {
                obstacle.OnPlayerAttack();
                break;
            }
        }
    }

    private IEnumerator HandleLandingDelay()
    {
        controller.SetCanMove(false);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Landing);

        yield return new WaitForSeconds(controller.data.moveDelayAfterFall);

        controller.SetCanMove(true);
        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
        controller.Rb.velocity = new Vector2(0, controller.Rb.velocity.y);
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(controller.MovementGroundCheckPoint.position, 0.1f, controller.MovementGroundLayer);
    }

    public void ApplySpeedDebuff(float multiplier, float duration)
    {
        if (isSlowed) return;

        speedDebuffCoroutine = StartCoroutine(SpeedDebuffRoutine(multiplier, duration));
    }

    private IEnumerator SpeedDebuffRoutine(float multiplier, float duration)
    {
        isSlowed = true;

        float originalRate = speedChangeRate;
        speedChangeRate *= multiplier;

        yield return new WaitForSeconds(duration);

        speedChangeRate = originalRate;
        isSlowed = false;
        speedDebuffCoroutine = null;
    }

    public void EnterSliding()
    {
        if (isSliding) return;

        isSliding = true;

        float dir = Mathf.Sign(controller.Rb.velocity.x);
        if (Mathf.Abs(dir) < 0.01f) dir = 1f;

        if (Mathf.Abs(controller.Rb.velocity.x) < 0.5f)
        {
            controller.Rb.velocity = new Vector2(dir * controller.data.maxSpeed * 0.3f, controller.Rb.velocity.y);
        }
        else
        {
            float slideForce = controller.data.maxSpeed * 2f;
            controller.Rb.velocity = new Vector2(dir * slideForce, controller.Rb.velocity.y);
        }

        // 마찰 제거
        controller.Rb.sharedMaterial = slipperyMaterial;
        controller.Rb.drag = 0f;
    }

    public void ExitSliding()
    {
        isSliding = false;
        controller.Rb.sharedMaterial = originalMaterial;
        controller.Rb.drag = originalDrag;
    }

    public float GetCurrentSpeed() => currentSpeed;
    public void StartMoving() => isHoldingClick = true;
    public bool IsJumping() => _isJumping;
    public bool IsSignificantFall() => _isSignificantFall;
    public IEnumerator HandleLandingDelayExternally() => HandleLandingDelay();
    
    public float SpeedChangeRate
    {
        get { return speedChangeRate; }
        set { speedChangeRate = value; }
    }
}