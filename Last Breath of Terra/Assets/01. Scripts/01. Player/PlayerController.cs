using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 움직임을 관리하는 스크립트
/// </summary>

public class PlayerController : MonoBehaviour
{
    public PlayerSO data;
    public GameObject clickIndicator;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 targetPosition;
    private float accelerationTimer;
    private bool isGrounded = true;
    private bool isMoving = false;
    private bool canMove = true;

    [SerializeField] private float currentSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentSpeed = data.baseSpeed;
        originalScale = transform.localScale;
    }

    void Update()
    {
        HandleAcceleration();

        if (isMoving && canMove)
        {
            Move();
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
        else
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        isMoving = false;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _animator.SetBool("Walk", false);

        if (clickIndicator != null)
        {
            clickIndicator.SetActive(false);
        }
    }


    private void HandleAcceleration()
    {
        if (isMoving)
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

    #region InputSystem
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            targetPosition = new Vector2(worldPosition.x, transform.position.y);

            if (Vector2.Distance(targetPosition, transform.position) > 0.1f)
            {
                isMoving = true;
                _animator.SetBool("Walk", true);

                if (clickIndicator != null)
                {
                    clickIndicator.transform.position = worldPosition;
                    clickIndicator.SetActive(true);
                }
            }
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        StopMoving();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isMoving && isGrounded && canMove)
        {
            _rb.AddForce(Vector2.up * data.jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
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

    public void SetCanMove(bool value)
    {
        if (!value)
        {
            _animator.SetBool("Walk", false);
        }
        canMove = value;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
