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

    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 targetPosition;
    private float accelerationTimer;
    private bool isGrounded = true;
    private bool canMove = true;
    private bool isHoldingClick = false;
    private string currentMapType;

    [SerializeField] private float currentSpeed;
    private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentSpeed = data.baseSpeed;
        originalScale = transform.localScale;

        currentMapType = GetCurrentMapType();
    }

    void Update()
    {
        HandleAcceleration();

        if (isHoldingClick && canMove)
        {
            Move();
            HandleFootstepSound();
        }
    }

    void FixedUpdate()
    {
        if (isHoldingClick && canMove)
        {
            UpdateTargetPosition();
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
        isHoldingClick = false;
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        _animator.SetBool("Walk", false);
        PlayStoppingFootstepSound();
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

    private void HandleFootstepSound()
    {
        footstepTimer += Time.deltaTime;

        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.instance.PlayFootstepSFX(currentMapType, gameObject.GetComponent<AudioSource>(), false);
        }
    }

    private void PlayStoppingFootstepSound()
    {
        AudioManager.instance.PlayFootstepSFX(currentMapType, gameObject.GetComponent<AudioSource>(), true);
    }


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
        StopMoving();
        UIManager.Instance.ReleaseClick();
        AudioManager.instance.StopLight(gameObject.GetComponent<AudioSource>(), transform);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && canMove)
        {
            _rb.AddForce(Vector2.up * data.jumpForce, ForceMode2D.Impulse);
            isGrounded = false;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            UIManager.Instance.HandleJumpLight(worldPosition);
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
            UIManager.Instance.HandleClickLight(worldPosition);
        }
    }

    public void SetCanMove(bool value)
    {
        if (!value)
        {
            _animator.SetBool("Walk", false);
            UIManager.Instance.ReleaseClick();
        }
        canMove = value;
    }


    //따로 나중에 스크립트 빼는게 나을 것 같음.
    private string GetCurrentMapType()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("AlphaTest"))
        {
            return "AlphaTest";
        }
        else if (sceneName.Contains("sand"))
        {
            return "sand";
        }
        else if (sceneName.Contains("wood"))
        {
            return "wood";
        }

        return "default";
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
