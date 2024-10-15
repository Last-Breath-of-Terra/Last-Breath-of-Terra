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

    private Rigidbody2D rb;
    private Animator _animator;
    private Vector2 moveDirection;
    private bool isGrounded = true;

    [SerializeField] private float currentSpeed;
    private float accelerationTimer;


    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        _animator = this.GetComponent<Animator>();

        currentSpeed = baseSpeed;
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
        rb.velocity = new Vector2(moveDirection.x * currentSpeed, rb.velocity.y);
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
                isGrounded = false;
            }
        }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
