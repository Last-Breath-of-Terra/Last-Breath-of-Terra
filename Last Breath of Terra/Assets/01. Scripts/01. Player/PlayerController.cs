using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 움직임을 관리하는 스크립트
/// </summary>

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;

    private Rigidbody2D rb;
    private Animator _animator;
    private Vector2 moveDirection;


    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        _animator = this.GetComponent<Animator>();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }

    #region InpuySystem
        void OnMove(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();
            if(input != null)
            {
                moveDirection = new Vector2(input.x, 0f);
                //_animator.SetFloat("MoveSpeed", input.magnitude);
            }
        }

        void OnJump()
        {

        }
    #endregion
}
