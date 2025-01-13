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
    public bool canMove = true;

    [SerializeField] private float currentSpeed;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 targetPosition;
    private float accelerationTimer;
    private bool isHoldingClick = false;
    private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    private bool isOnWall = false;
    private bool isClimbing = false;
    private float climbSpeed = 3f;
    private float fallStartY = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentSpeed = data.baseSpeed;
        originalScale = transform.localScale;
        hp = data.hp;
    }

    void Update()
    {
        HandleAcceleration();

        if (isOnWall)
        {
            HandleWallActions();
        }
        else if (isHoldingClick && canMove)
        {
            UpdateTargetPosition();
            Move();
            HandleFootstepSound();
        }

        if (!isGrounded && _rb.velocity.y < 0f && fallStartY == 0f)
        {
            fallStartY = transform.position.y;
        }
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
        // 마지막 이전 버전
        if (isHoldingClick)
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

            currentSpeed = Mathf.Lerp(data.baseSpeed, data.maxSpeed, accelerationTimer / data.accelerationTime);
        }
        else
        {
            accelerationTimer = 0f;
            currentSpeed = data.baseSpeed;
        }

        // 이전 버전
        // if (isHoldingClick)
        // {
        //     accelerationTimer += Time.deltaTime;
        //     currentSpeed = Mathf.Lerp(data.baseSpeed, data.maxSpeed, accelerationTimer / data.accelerationTime);
        // }
        // else
        // {
        //     accelerationTimer = 0f;
        //     currentSpeed = data.baseSpeed;
        // }
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
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.instance.PlayRandomPlayer("footstep_" + GameManager.Map.GetCurrentMapType() + "_", 0);// gameObject.GetComponent<AudioSource>(), transform);
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, LayerMask.GetMask("Wall"));
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

        Debug.Log(targetPosition);

        transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
        {
            ResetWallState();
        });
    }

    private void StickToWall()
    {
        isOnWall = true;
        isClimbing = false;

        _animator.SetBool("Walk", false);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        //_animator.SetBool("StickToWall", true);
    }

    private void ClimbWall()
    {
        if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
            return;
        }

        isClimbing = true;
        _rb.velocity = new Vector2(0, climbSpeed);
        //_animator.SetBool("Climb", true);
    }

    private void FallOffWall()
    {
        isOnWall = false;
        isClimbing = false;

        //나중에 콜라이더 문제 해결이 안 될 경우 활성화
        //isGrounded = false;

        _rb.gravityScale = 3f;
       
        float backwardForce = 2f;
        float direction = transform.localScale.x > 0 ? -1 : 1;

        _rb.velocity = new Vector2(backwardForce * direction, -2f);

        //_animator.SetBool("StickToWall", false);
        //_animator.SetBool("Climb", false);
    }

    private void ResetWallState()
    {
        isOnWall = false;
        isClimbing = false;

        _rb.gravityScale = 3f;
        _rb.velocity = Vector2.zero;
    }
    #endregion

    #region InputSystem
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        
        if (context.performed && canMove)
        {
            Invoke("StartMoving", 0.3f);
            UpdateTargetPosition();
        }
    }

    private void StartMoving()
    {
        if (canMove)
        {
            isHoldingClick = true;
            _animator.SetBool("Walk", true);
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isHoldingClick = false;

        GameManager.Instance._ui.ReleaseClick();
        
        if (isOnWall)
        {
            if (isClimbing)
            {
                FallOffWall();
            }
            else
            {
                isOnWall = false;
                isClimbing = false;
                _rb.gravityScale = 3f;
                _rb.velocity = Vector2.zero;
            }
        }

        _rb.velocity = new Vector2(0, _rb.velocity.y);

        _animator.SetBool("Walk", false);
        _animator.SetBool("Run", false);

        AudioManager.instance.StopCancelable(gameObject.GetComponent<AudioSource>());
        AudioManager.instance.PlaySFX("footstep_" + GameManager.Map.GetCurrentMapType() + "_4", gameObject.GetComponent<AudioSource>(), transform);

        CancelInvoke("StartMoving");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!canMove || isClimbing)
            return;

        if (isGrounded && canMove)
        {
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
            isGrounded = false;

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
            
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                Obstacle obstacle = hit.collider.GetComponent<Obstacle>();

                if (obstacle == null)
                {
                    obstacle = hit.collider.transform.parent?.GetComponent<Obstacle>();
                }

                if (obstacle != null)
                {
                    obstacle.OnPlayerAttack();
                }
            }
            
            // if (hit.collider != null)
            // {
            //     Obstacle obstacle = hit.collider.transform.parent?.GetComponent<Obstacle>();
            //     if (obstacle != null)
            //     {
            //         obstacle.OnPlayerAttack();
            //     }
            // }
        }
    }
    #endregion

    #region Moving System
    private void Move()
    {
        if (!canMove || isOnWall) return;

        float distanceX = Mathf.Abs(targetPosition.x - transform.position.x);

        if (distanceX > 0.5f)
        {
            // 기존 방법
            float direction = (targetPosition.x - transform.position.x) > 0 ? 1 : -1;

            if (isGrounded)
            {
                transform.localScale = new Vector3(direction * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            }
            else
            {
                direction = transform.localScale.x > 0 ? 1 : -1;
            }

            _rb.velocity = new Vector2(direction * currentSpeed, _rb.velocity.y);

            UpdateAnimationState();

        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            _rb.angularVelocity = 0;

            currentSpeed = data.baseSpeed;

            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
        }
    }

    private void UpdateTargetPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (Vector2.Distance(worldPosition, transform.position) <= 0.1f)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        targetPosition = worldPosition;

        if (Vector2.Distance(targetPosition, transform.position) > 0.1f)
        {
            GameManager.Instance._ui.HandleClickLight(worldPosition);
        }
    }

    public void SetCanMove(bool value)
    {
        if (!value)
        {
            _animator.SetBool("Walk", false);
            GameManager.Instance._ui.ReleaseClick();
        }
        canMove = value;
    }

    private IEnumerator HandleLandingDelay()
    {
        //_animator.SetBool("Land", true);
        isGrounded = true;
        canMove = false;

        yield return new WaitForSeconds(3f);

        //_animator.SetBool("Land", false);
        canMove = true;

         _rb.velocity = new Vector2(0, _rb.velocity.y);
    }
    #endregion

    private void UpdateAnimationState()
    {
        if (!canMove || isOnWall)
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
            return;
        }

        if (currentSpeed > 5f)
        {
            if (!_animator.GetBool("Run"))
            {
                _animator.SetBool("Run", true);
                _animator.SetBool("Walk", false);
            }
        }
        else if (currentSpeed > 2f)
        {
            if (!_animator.GetBool("Walk"))
            {
                _animator.SetBool("Walk", true);
                _animator.SetBool("Run", false);
            }
        }
        else
        {
            if (_animator.GetBool("Walk") || _animator.GetBool("Run"))
            {
                _animator.SetBool("Walk", false);
                _animator.SetBool("Run", false);
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!isOnWall && _rb.velocity.y <= 0)
            {
                StickToWall();
            }
        }
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded && _rb.velocity.y <= 0)
            {
                float groundY = collision.contacts[0].point.y;
                if (groundY < transform.position.y)
                {
                    float fallHeight = fallStartY - transform.position.y;
                    fallStartY = 0f;

                    if (fallHeight > 5f)
                    {
                        StartCoroutine(HandleLandingDelay());
                    }
                    else
                    {
                        isGrounded = true;
                    }
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
            isGrounded = false;

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
