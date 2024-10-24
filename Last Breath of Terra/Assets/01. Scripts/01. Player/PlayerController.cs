using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 움직임을 관리하는 스크립트
/// </summary>

public class PlayerController : MonoBehaviour
{
    public float baseSpeed = 1f;
    public float maxSpeed = 5f;
    public float accelerationTime = 3f;
    public float jumpForce = 5f;
    public float dashForce = 10f;

    private Rigidbody2D rb;
    private Animator _animator;
    private InputAction attackAction;
    private Vector2 moveDirection;
    private float jumpStartHeight;
    private bool isGrounded = true;
    private bool canDash = false;

    [SerializeField] private float currentSpeed;
    private float accelerationTimer;


    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        _animator = this.GetComponent<Animator>();

        currentSpeed = baseSpeed;
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions["Attack"];
        attackAction.performed += PerformAttack;
    }

    private void OnDisable()
    {
        attackAction.performed -= PerformAttack;
    }

    void Update()
    {
        HandleAcceleration();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (moveDirection != Vector2.zero)
        {
            rb.velocity = new Vector2(moveDirection.x * currentSpeed, rb.velocity.y);
        }
        else if (isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void HandleAcceleration()
    {
        if (moveDirection != Vector2.zero)
        {
            accelerationTimer += Time.deltaTime;
            currentSpeed = Mathf.Lerp(baseSpeed, maxSpeed, accelerationTimer / accelerationTime);
        }
        else
        {
            accelerationTimer = 0f;
            currentSpeed = baseSpeed;
        }
    }

    #region InpuySystem
        void OnMove(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();
            if(input != null)
            {
                moveDirection = new Vector2(input.x, 0f);
            }
        }

        void OnJump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpStartHeight = transform.position.y;
                isGrounded = false;
                canDash = true;
            }
        }

        private void PerformAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

                RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
                if (hit.collider != null)
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.OnPlayerAttack();
                    }
                }
            }
        }

        // void OnSlam()
        // {
        //     if (!isGrounded)
        //     {
        //         float fallHeight = transform.position.y - jumpStartHeight;
        //         float dynamicSlamForce = Mathf.Clamp(fallHeight * 10f, 10f, 50f);
        //         rb.velocity = new Vector2(rb.velocity.x, 0);
        //         rb.AddForce(Vector2.down * dynamicSlamForce, ForceMode2D.Impulse);
        //     }
        // }

        // void OnDash()
        // {
        //     if (canDash)
        //     {
        //         Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //         Vector2 dashDirection = (mousePosition - (Vector2)transform.position).normalized;
        //         rb.velocity = Vector2.zero;
        //         rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        //         canDash = false;
        //     }
        // }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            canDash = false;
        }
    }
}
