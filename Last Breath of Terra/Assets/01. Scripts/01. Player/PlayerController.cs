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

    [SerializeField] private float currentSpeed;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 targetPosition;
    private float accelerationTimer;
    private bool canMove = true;
    private bool isHoldingClick = false;
    private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    private bool isOnWall = false;
    private bool isClimbing = false;
    private float climbSpeed = 3f;

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

    private void Move()
    {
        float distanceX = Mathf.Abs(targetPosition.x - transform.position.x);

        if (distanceX > 0.1f)
        {
            float direction = (targetPosition.x - transform.position.x) > 0 ? 1 : -1;
            _rb.velocity = new Vector2(direction * currentSpeed, _rb.velocity.y);

            transform.localScale = new Vector3(direction * Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    private void HandleAcceleration()
    {
        if (isHoldingClick)
        {
            accelerationTimer += Time.deltaTime;
            currentSpeed = Mathf.Lerp(data.baseSpeed, data.maxSpeed, accelerationTimer / data.accelerationTime);
        }
        else
        {
            accelerationTimer = 0f;
            currentSpeed = data.baseSpeed;
        }
    }

    private void HandleWallActions()
    {
        Vector2 worldPosition = GameManager.Instance._ui.GetMouseWorldPosition();

        if (IsAtWallTop())
        {
            AutoMoveAfterWallTop();
            return;
        }
        else if (worldPosition.y > transform.position.y + 0.5f && !isClimbing)
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
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        //_animator.SetBool("StickToWall", true);
    }

    private void ClimbWall()
    {
        if (IsAtWallTop())
        {
            ResetWallState();
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
        _rb.gravityScale = 3f;
        _rb.velocity = Vector2.zero;
        //_animator.SetBool("StickToWall", false);
        //_animator.SetBool("Climb", false);
    }

    private void ResetWallState()
    {
        isOnWall = false;
        isClimbing = false;
        _rb.gravityScale = 3f;
        _rb.velocity = Vector2.zero;
        //_animator.SetBool("StickToWall", false);
        //_animator.SetBool("Climb", false);
    }
    #endregion

    #region InputSystem
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
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
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _animator.SetBool("Walk", false);
        AudioManager.instance.StopCancelable(gameObject.GetComponent<AudioSource>());
        AudioManager.instance.PlaySFX("footstep_" + GameManager.Map.GetCurrentMapType() + "_4", gameObject.GetComponent<AudioSource>(), transform);
        GameManager.Instance._ui.ReleaseClick();

        CancelInvoke("StartMoving");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isClimbing)
            return;

        if (isGrounded && canMove)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 0);

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

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            if (hit.collider != null)
            {
                Obstacle obstacle = hit.collider.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.OnPlayerAttack();
                }
            }
        }
    }
    #endregion

    private void UpdateTargetPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (!isOnWall && _rb.velocity.y <= 0)
            {
                StickToWall();
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            FallOffWall();
        }
    }
}
