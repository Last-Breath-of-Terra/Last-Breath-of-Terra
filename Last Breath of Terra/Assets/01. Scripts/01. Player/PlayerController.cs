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
    public PlayerSO data;
    public float hp;
    public bool isGrounded = true;
    public bool isJumping;
    public bool canMove = true;

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 targetPosition;
    private float accelerationTimer;
    private float fallStartY = 0f;
    private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    private bool isHoldingClick = false;
    private bool isOnWall = false;
    private bool isClimbing = false;
    private bool isFallingDelay = false;
    private bool isSignificantFall = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        originalScale = transform.localScale;
        hp = data.hp;
    }

    void Update()
    {
        UpdateAnimationState();

        if (!canMove) return;

        HandleAcceleration();

        if (isOnWall)
        {
            HandleWallActions();
        }
        else if (isHoldingClick)
        {
            UpdateTargetPosition();
            Move();
            HandleFootstepSound();
        }

        if (!isGrounded && _rb.velocity.y < 0f && fallStartY == 0f)
        {
            fallStartY = transform.position.y;
        }

        HandleFalling();
    }

    void FixedUpdate()
    {
        UpdateGroundedState();
    }
    
    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Move"].performed += OnMovePerformed;
        playerInput.actions["Move"].canceled += OnMoveCanceled;
        playerInput.actions["Jump"].performed += OnJumpPerformed;
        playerInput.actions["Attack"].performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Move"].performed -= OnMovePerformed;
        playerInput.actions["Move"].canceled -= OnMoveCanceled;
        playerInput.actions["Jump"].performed -= OnJumpPerformed;
        playerInput.actions["Attack"].performed -= OnAttackPerformed;
    }

    #region Handle System
    private void HandleAcceleration()
    {   
        if (isHoldingClick && !isFallingDelay && !isClimbing)
        {
            float direction = (targetPosition.x - transform.position.x) > 0 ? 1 : -1;

            if ((direction > 0 && _rb.velocity.x < 0) || (direction < 0 && _rb.velocity.x > 0))
            {
                accelerationTimer -= Time.deltaTime * 2f;
                if (accelerationTimer < 0) accelerationTimer = 0f;
            }
            else
            {
                accelerationTimer += Time.deltaTime;
            }

            currentSpeed = Mathf.Lerp(0f, data.maxSpeed, accelerationTimer / data.accelerationTime);
        }
        else
        {
            accelerationTimer = 0f;
            currentSpeed = 0f;
        }
    }

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

    private void HandleFootstepSound()
    {
        if (!isGrounded || _rb.velocity.magnitude < 0.1f)
            return;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.instance.PlayRandomPlayer("footstep_" + GameManager.ScenesManager.GetCurrentSceneType() + "_", 0);// gameObject.GetComponent<AudioSource>(), transform);
        }
    }
    #endregion

    #region Wall Climbing System
    private bool IsAtWallTop()
    {
        float wallTopY = GetWallTopY();

        if (wallTopY == float.MaxValue)
        {
            return true;
        }

        return transform.position.y >= wallTopY;
    }

    private float GetWallTopY()
    {
        Collider2D wallCollider = GetWallCollider();
        if (wallCollider != null)
        {
            return wallCollider.bounds.max.y;
        }
        return float.MaxValue;
    }

    private Collider2D GetWallCollider()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.5f, LayerMask.GetMask("Wall"));
        return hit.collider;
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

        Vector3 targetPosition = new Vector3(
            transform.position.x + (forwardDistance * direction),
            transform.position.y + upwardDistance,
            transform.position.z
        );

        _animator.SetBool("isClimbing", false);

        transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
        {
            ResetWallState();
        });
    }

    private void StickToWall()
    {
        isOnWall = true;
        isClimbing = false;

        _animator.SetBool("isJumping", false);
        _animator.SetBool("isClimbing", true);
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
    }

    private void ClimbWall()
    {
        if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
            return;
        }

        isClimbing = true;
        _rb.velocity = new Vector2(0, data.climbSpeed);
    }

    private void FallOffWall()
    {
        isOnWall = false;
        isClimbing = false;
        canMove = false;
        isFallingDelay = true;

        _rb.gravityScale = 3f;
        _animator.SetBool("isFalling", true);
        _animator.SetBool("isClimbing", false);
       
        float backwardForce = 2f;
        float direction = transform.localScale.x > 0 ? -1 : 1;

        _rb.velocity = new Vector2(backwardForce * direction, -2f);

        StartCoroutine(EnableMovementAfterDelay());
    }

    private IEnumerator EnableMovementAfterDelay()
    {
        yield return new WaitForSeconds(data.moveDelayAfterFall);

        _animator.SetBool("isLanding", false);
        canMove = true;
        isFallingDelay = false;
    }

    private void ResetWallState()
    {
        isOnWall = false;
        isClimbing = false;

        _animator.SetBool("isClimbing", false);

        _rb.gravityScale = 3f;
        _rb.velocity = Vector2.zero;
    }
    #endregion

    #region Input System
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (context.performed && canMove)
        {
            Invoke("StartMoving", 0.3f);
        }
    }

    private void StartMoving()
    {
        isHoldingClick = true;
        UpdateTargetPosition();
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
        AudioManager.instance.PlaySFX("footstep_" + GameManager.ScenesManager.GetCurrentSceneType() + "_4", gameObject.GetComponent<AudioSource>(), transform);

        CancelInvoke("StartMoving");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!canMove || isClimbing)
            return;

        if (isGrounded && canMove)
        {
            isJumping = true;
            _animator.SetBool("isJumping", true);

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

            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);
            
            Obstacle detectedObstacle = null;
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("obstacleHover"))
                {
                    detectedObstacle = hit.collider.GetComponentInParent<Obstacle>();
                    break;
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("obstacle"))
                {
                    detectedObstacle = hit.collider.GetComponent<Obstacle>();
                }
            }

            if (detectedObstacle != null)
            {
                detectedObstacle.OnPlayerAttack();
            }
        }
    }
    #endregion

    #region Moving System
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
        float distanceX = Mathf.Abs(worldPosition.x - transform.position.x);

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

    private void HandleFalling()
    {
        if (!isGrounded && _rb.velocity.y < 0f)
        {
            if (!isOnWall && !isClimbing)
            {
                _animator.SetBool("isJumping", false);
                _animator.SetBool("isFalling", true);
            }

            if (fallStartY == 0f)
            {
                fallStartY = transform.position.y;
            }

            if (fallStartY - transform.position.y > 2f)
            {
                isSignificantFall = true;
            }
        }
        else if(isGrounded)
        {
            isSignificantFall = false;
        }
        else
        {
            if (isGrounded || _rb.velocity.y >= 0f)
            {
                _animator.SetBool("isFalling", false);
            }
        }
    }

    private IEnumerator HandleLandingDelay()
    {
        if (!isGrounded || _rb.velocity.y < 0) yield break;
        canMove = false;

        yield return new WaitForSeconds(data.moveDelayAfterFall);
        canMove = true;
        _animator.SetBool("isLanding", false);

        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }
    #endregion

    private void UpdateGroundedState()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.1f, groundLayer);
    }
    private void UpdateAnimationState()
    {
        _animator.SetFloat("currentSpeed", currentSpeed);
    }

    #region External Control System
    public void SetActivatingState(bool isActivating)
    {
        _animator.SetBool("isActivating", isActivating);
    }
    public void SetKnockdownState(bool isKnockdown)
    {
        _animator.SetBool("isKnockdown", isKnockdown);
    }
    public void SetCanMove(bool value)
    {
        if (!value)
        {
            GameManager.Instance._ui.ReleaseClick();
        }
        canMove = value;
    }
    #endregion

    public bool IsSignificantFall()
    {
        return isSignificantFall;
    }

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
                    AudioManager.instance.PlaySFX("footstep_" + GameManager.ScenesManager.GetCurrentSceneType() + "_4", gameObject.GetComponent<AudioSource>(), transform);
                    _animator.SetBool("isFalling", false);

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
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (isOnWall)
            {
                FallOffWall();
            }
        }
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            fallStartY = transform.position.y;

            if (Mathf.Abs(_rb.velocity.x) < 0.1f)
            {
                _rb.velocity = new Vector2(0, _rb.velocity.y);
            }
            else
            {
                float fallDirection = transform.localScale.x > 0 ? 1 : -1;
                _rb.velocity = new Vector2(fallDirection * currentSpeed, _rb.velocity.y);
            }
        }
    }
}
