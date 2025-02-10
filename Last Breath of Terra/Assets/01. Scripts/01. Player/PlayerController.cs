using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// 플레이어의 움직임을 관리하는 스크립트
/// </summary>

public class PlayerController : MonoBehaviour
{
    private enum AnimationState {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Landing,
        Climbing,
        Knockdown,
        Activating,
        MoveToPortal
    }
    private AnimationState currentAnimState = AnimationState.Idle;

    #region Fields

    [Header("Player Data")]
    public PlayerSO data;
    public float hp;

    [Header("Movement Settings")]
    public bool isGrounded = true;
    public bool isJumping = false;
    public bool canMove = true;

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    
    // Components
    private Rigidbody2D _rb;
    private Animator _animator;
    private AudioSource _audioSource;
    private PlayerInput _playerInput;

    // Initial state value
    private Vector3 originalScale;
    private Vector2 targetPosition;

    // Timers & Thresholds
    private float moveAccelerationTimer;
    private float fallStartY = 0f;
    private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    // Input and Movement State
    private bool isHoldingClick = false;
    private bool isOnWall = false;
    private bool isClimbing = false;
    private bool isFallingDelay = false;
    private bool isSignificantFall = false;
    #endregion

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        originalScale = transform.localScale;
        hp = data.hp;
    }

    void Update()
    {
        UpdateAnimatorParameters();

        if (!canMove) return;

        if (isHoldingClick)
        {
            UpdateTargetPosition();
        }

        if (!isGrounded && _rb.velocity.y < 0f && fallStartY == 0f)
        {
            fallStartY = transform.position.y;
        }
    }

    void FixedUpdate()
    {
        UpdateGroundedState();
        UpdateAcceleration();
        
        UpdateFalling();
        UpdateFallingSpeed();

        if (isOnWall)
        {
            HandleWallActions();
        }
        else if (isHoldingClick)
        {
            Move();
            UpdateFootstepSound();
        }
    }
    
    private void OnEnable()
    {
        _playerInput.actions["Move"].performed += OnMovePerformed;
        _playerInput.actions["Move"].canceled += OnMoveCanceled;
        _playerInput.actions["Jump"].performed += OnJumpPerformed;
        _playerInput.actions["Attack"].performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        _playerInput.actions["Move"].performed -= OnMovePerformed;
        _playerInput.actions["Move"].canceled -= OnMoveCanceled;
        _playerInput.actions["Jump"].performed -= OnJumpPerformed;
        _playerInput.actions["Attack"].performed -= OnAttackPerformed;
    }

    #region Update Methods
    private void UpdateAnimatorParameters()
    {
        _animator.SetFloat("currentSpeed", currentSpeed);
        UpdateAnimationState();
    }

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.1f, groundLayer);
    }

    private void UpdateAcceleration()
    {   
        if (isHoldingClick && !isFallingDelay && !isClimbing)
        {
            float direction = (targetPosition.x - transform.position.x) > 0 ? 1 : -1;

            if ((direction > 0 && _rb.velocity.x < 0) || (direction < 0 && _rb.velocity.x > 0))
            {
                moveAccelerationTimer -= Time.deltaTime * 2f;
                if (moveAccelerationTimer < 0) moveAccelerationTimer = 0f;
            }
            else
            {
                moveAccelerationTimer += Time.deltaTime;
            }

            currentSpeed = Mathf.Lerp(0f, data.maxSpeed, moveAccelerationTimer / data.moveAccelerationTime);
        }
        else
        {
            moveAccelerationTimer = 0f;
            currentSpeed = 0f;
        }
    }

    private void UpdateFalling()
    {
        if (!isGrounded && _rb.velocity.y < -1f)
        {
            if (!isOnWall && !isClimbing)
            {
                ChangeAnimationState(AnimationState.Fall);
            }

            if (fallStartY == 0f)
            {
                fallStartY = transform.position.y;
            }

            float fallHeight = fallStartY - transform.position.y;
            isSignificantFall = fallHeight > 2f;
        }
        else if(isGrounded)
        {
            isSignificantFall = false;
        }
    }

    private void UpdateFallingSpeed()
    {
        if (_rb.velocity.y < 0)
        {
            float newFallSpeed = _rb.velocity.y - data.fallAccelerationTime * Time.fixedDeltaTime;
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(newFallSpeed, -data.maxFallSpeed));
        }
    }

    private IEnumerator HandleLandingDelay()
    {
        if (!isGrounded || _rb.velocity.y < 0) yield break;
        canMove = false;
        ChangeAnimationState(AnimationState.Landing);

        yield return new WaitForSeconds(data.moveDelayAfterFall);
        canMove = true;
        ChangeAnimationState(AnimationState.Idle);

        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    private void UpdateFootstepSound()
    {
        if (!isGrounded || _rb.velocity.magnitude < 0.1f)
            return;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.instance.PlayRandomPlayer(GetFootstepClipPrefix(), 0);
        }
    }
    #endregion

    #region Animation State Machine Methods
    private void UpdateAnimationState() {
        // if (currentAnimState == AnimationState.Knockdown ||
        //     currentAnimState == AnimationState.Activating ||
        //     currentAnimState == AnimationState.MoveToPortal)
        //     return;

        // if (!isGrounded) {
        //     if (_rb.velocity.y < 0) {
        //         ChangeAnimationState(AnimationState.Fall);
        //     }
        // }
    }

    private void ChangeAnimationState(AnimationState newState) {
        if (currentAnimState == newState) return;
        currentAnimState = newState;

        _animator.ResetTrigger("Jump");
        _animator.ResetTrigger("Landing");
        _animator.SetBool("isKnockdown", false);
        _animator.SetBool("isActivating", false);
        _animator.SetBool("MoveToPortal", false);
        _animator.SetBool("isFalling", false);
        _animator.SetBool("isClimbing", false);

        switch (newState) {
            case AnimationState.Idle:
            case AnimationState.Walk:
            case AnimationState.Run:
                break;
            case AnimationState.Jump:
                _animator.SetTrigger("Jump");
                break;
            case AnimationState.Fall:
                _animator.SetBool("isFalling", true);
                break;
            case AnimationState.Landing:
                _animator.SetTrigger("Landing");
                break;
            case AnimationState.Climbing:
                _animator.SetBool("isClimbing", true);
                break;
            case AnimationState.Knockdown:
                _animator.SetBool("isKnockdown", true);
                break;
            case AnimationState.Activating:
                _animator.SetBool("isActivating", true);
                break;
            case AnimationState.MoveToPortal:
                _animator.SetBool("MoveToPortal", true);
                break;
        }
    }

    #endregion

    #region Wall Climbing Methods
    private void HandleWallActions()
    {
        if (!isOnWall || !isHoldingClick) return;

        Vector2 worldPosition = GameManager.Instance._ui.GetMouseWorldPosition();

        if (worldPosition.y > transform.position.y + 0.5f)
        {
            ClimbWall();
        }

        else if (worldPosition.y < transform.position.y - 0.5f)
        {
            FallOffWall();
        }
    }

    private void ClimbWall()
    {
        if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
            return;
        }

        isClimbing = true;
        _rb.velocity = new Vector2(0f, data.climbSpeed);
    }

    private void FallOffWall()
    {
        isOnWall = false;
        isClimbing = false;
        canMove = false;
        isFallingDelay = true;

        _rb.gravityScale = 3f;
        ChangeAnimationState(AnimationState.Fall);
       
        float backwardForce = 2f;
        float direction = transform.localScale.x > 0 ? -1f : 1f;

        _rb.velocity = new Vector2(backwardForce * direction, -2f);

        StartCoroutine(EnableMovementAfterDelay());
    }

    private IEnumerator EnableMovementAfterDelay()
    {
        yield return new WaitForSeconds(data.moveDelayAfterFall);

        ChangeAnimationState(AnimationState.Landing);
        canMove = true;
        isFallingDelay = false;
    }

    private void AutoMoveAfterWallTop()
    {
        if (!isClimbing) return;

        isClimbing = false;
        isOnWall = false;
        _rb.gravityScale = 0f;

        float upwardDistance = 2f;
        float forwardDistance = 0.5f;
        float direction = transform.localScale.x > 0 ? 1 : -1;

        Vector3 targetPos = new Vector3(
            transform.position.x + (forwardDistance * direction),
            transform.position.y + upwardDistance,
            transform.position.z
        );

        transform.DOMove(targetPos, 0.5f).OnComplete(() =>
        {
            ResetWallState();
        });
    }

    private void StickToWall()
    {
        isOnWall = true;
        isClimbing = false;

        ChangeAnimationState(AnimationState.Climbing);
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
    }

    private bool IsAtWallTop()
    {
        float wallTopY = GetWallTopY();

        return wallTopY == float.MaxValue || transform.position.y >= wallTopY;
    }

    private float GetWallTopY()
    {
        Collider2D wallCollider = GetWallCollider();
        return wallCollider != null ? wallCollider.bounds.max.y : float.MaxValue;
    }

    private Collider2D GetWallCollider()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.5f, LayerMask.GetMask("Wall"));
        return hit.collider;
    }

    private void ResetWallState()
    {
        _rb.gravityScale = 3f;
        _rb.velocity = Vector2.zero;
        ChangeAnimationState(AnimationState.Idle);
    }
    #endregion

    #region Input Handlers
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (context.performed && canMove)
        {
            Invoke("StartMoving", 0.3f);
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isHoldingClick = false;
        GameManager.Instance._ui.ReleaseClick();
        
        if (isOnWall)
        {
            FallOffWall();
        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }

        AudioManager.instance.StopCancelable(gameObject.GetComponent<AudioSource>());
        AudioManager.instance.PlaySFX(GetFootstepClipPrefix() + "4", gameObject.GetComponent<AudioSource>(), transform);

        CancelInvoke("StartMoving");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!canMove || isClimbing)
            return;

        if (isGrounded)
        {
            isJumping = true;
            ChangeAnimationState(AnimationState.Jump);

            fallStartY = transform.position.y;

            float direction = transform.localScale.x > 0 ? 1 : -1;
            if (currentSpeed > 2.1f)
            {
                _rb.velocity = new Vector2(direction * currentSpeed, 0);
            }
            else
            {
                _rb.velocity = new Vector2(_rb.velocity.x, 0);
            }

            _rb.AddForce(Vector2.up * data.jumpForce, ForceMode2D.Impulse);

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            GameManager.Instance._ui.HandleJumpLight(worldPosition);
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            Obstacle obstacle = DetectObstacleAtPosition(worldPosition);
            if (obstacle != null)
            {
                obstacle.OnPlayerAttack();
            }
        }
    }

    private void StartMoving()
    {
        isHoldingClick = true;
        UpdateTargetPosition();
    }

    private Obstacle DetectObstacleAtPosition(Vector2 worldPosition)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);
        Obstacle detectedObstacle = null;
        foreach (var hit in hits)
        {
            int layer = hit.collider.gameObject.layer;
            if (layer == LayerMask.NameToLayer("obstacleHover"))
            {
                detectedObstacle = hit.collider.GetComponentInParent<Obstacle>();
                break;
            }
            if (layer == LayerMask.NameToLayer("obstacle"))
            {
                detectedObstacle = hit.collider.GetComponent<Obstacle>();
            }
        }
        return detectedObstacle;
    }
    #endregion

    #region Movement Methods
    private void Move()
    {
        if (!canMove || isOnWall || isFallingDelay) return;

        float distanceX = Mathf.Abs(targetPosition.x - transform.position.x);
        float direction = (targetPosition.x - transform.position.x) > 0 ? 1 : -1;

        if (distanceX > 0.5f)
        {
            if (isGrounded)
            {
                transform.localScale = new Vector3(direction * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
            else
            {
                direction = transform.localScale.x > 0 ? 1 : -1;
            }

            _rb.velocity = new Vector2(direction * currentSpeed, _rb.velocity.y);

        }
        else
        {            
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            currentSpeed = 0f;
        }
    }

    private void UpdateTargetPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //float distanceX = Mathf.Abs(worldPosition.x - transform.position.x);

        if (Vector2.Distance(worldPosition, transform.position) <= 0.1f)
        {
            return;
        }

        targetPosition = worldPosition;

        if (Vector2.Distance(targetPosition, transform.position) > 0.1f)
        {
            GameManager.Instance._ui.HandleClickLight(worldPosition);
        }
    }
    #endregion

    #region External Control Methods
    public void SetActivatingState(bool isActivating)
    {
        if (isActivating)
            ChangeAnimationState(AnimationState.Activating);
        else
            ChangeAnimationState(AnimationState.Idle);
    }
    public void SetKnockdownState(bool isKnockdown)
    {
        if (isKnockdown)
            ChangeAnimationState(AnimationState.Knockdown);
        else
            ChangeAnimationState(AnimationState.Idle);
    }
    public void SetCanMove(bool value)
    {
        if (!value)
        {
            GameManager.Instance._ui.ReleaseClick();
        }
        canMove = value;
    }

    public bool IsSignificantFall()
    {
        return isSignificantFall;
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if(!isOnWall)
            {
                StickToWall();
            }
        }
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y < transform.position.y)
                {
                    AudioManager.instance.PlaySFX(GetFootstepClipPrefix() + "4", _audioSource, transform);
                    ChangeAnimationState(AnimationState.Idle);
                    isJumping = false;

                    float fallHeight = fallStartY - transform.position.y;
                    if (fallHeight > 5f)
                    {
                        StartCoroutine(HandleLandingDelay());
                    }
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && isOnWall)
        {
            FallOffWall();
        }
        
        else if (collision.gameObject.CompareTag("Ground"))
        {
            fallStartY = transform.position.y;

            if (Mathf.Abs(_rb.velocity.x) < 0.1f)
            {
                _rb.velocity = new Vector2(0f, _rb.velocity.y);
            }
            else
            {
                float fallDirection = transform.localScale.x > 0 ? 1f : -1f;
                _rb.velocity = new Vector2(fallDirection * currentSpeed, _rb.velocity.y);
            }
        }
    }

    #region Helper Methods
    private string GetFootstepClipPrefix()
    {
        return "footstep_" + GameManager.ScenesManager.GetCurrentSceneType() + "_";
    }
    #endregion
}
