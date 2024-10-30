using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 장애물의 기능을 담당하고 있는 클래스
/// </summary>

public class Obstacle : MonoBehaviour
{
    public ObstacleSO data;
    public Transform[] attackPoints;
    public Transform timingIndicator;
    public GameObject attackGroup;

    private Rigidbody2D _rb;
    private Transform player; //플레이어 변수는 GameManager에서 가져오는 등 할 예정
    private Vector3 initialTimingIndicatorPos;
    private int currentHitCount = 0;
    private float currentSpeed;
    private bool isHovered = false;
    private bool isActive = true;
    private bool isTimingCorrect = false;
    private bool hasCompletedFullRotation = false;

    private void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialTimingIndicatorPos = timingIndicator.localPosition;
    }

    private void OnEnable()
    {
        isActive = true;
        ObstacleManager.Instance.RegisterObstacle(this);

        currentHitCount = 0;
        currentSpeed = data.speed;
    }

    private void OnDisable()
    {
        isActive = false;
        ObstacleManager.Instance.UnregisterObstacle(this);
    }

    private void Update()
    {
        if (isActive)
        {
            MoveTowardsPlayer();
            HandleMouseHover();
        }
    }

    #region Movement Methods
    private void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > data.stopDistance)
            {
                transform.position += direction * currentSpeed * Time.deltaTime;
            }
        }
    }
    #endregion

    #region Hover and Attack Methods
    private void HandleMouseHover()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        bool isCurrentlyHovered = hit.collider != null && hit.collider.gameObject == gameObject;
        if (isHovered != isCurrentlyHovered)
        {
            isHovered = isCurrentlyHovered;
            if (isHovered)
            {
                ObstacleManager.Instance.SlowDownAllObstacles();
            }
            else
            {
                ObstacleManager.Instance.ResetAllObstaclesSpeed();
            }
        }

        HandleHoverEffect();
    }

    private void HandleHoverEffect()
    {
        attackGroup.SetActive(isHovered);

        if (isHovered)
        {
            RotateTimingIndicator();
        }
        else
        {
            ResetAttackState();
        }
    }

    private void RotateTimingIndicator()
    {
        if (timingIndicator != null)
        {
            timingIndicator.RotateAround(transform.position, Vector3.back, data.timingRotationSpeed * Time.deltaTime);
            if (Vector3.Distance(timingIndicator.localPosition, initialTimingIndicatorPos) < 0.1f && hasCompletedFullRotation)
            {
                if (currentHitCount < 3)
                {
                    ResetAttackState();
                }
                hasCompletedFullRotation = false;
            }
            else if (Vector3.Distance(timingIndicator.localPosition, initialTimingIndicatorPos) > 0.1f)
            {
                hasCompletedFullRotation = true;
            }

            CheckTiming();
        }
    }

    private void CheckTiming()
    {
        isTimingCorrect = false;
        foreach (Transform point in attackPoints)
        {
            //이부분은 장애물의 오브젝트 크기에 따라 체크 범위가 달라지는 문제가 있어서 코드 수정이 필요함
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.1f)
            {
                isTimingCorrect = true;
                break;
            }
        }
    }

    public void OnPlayerAttack()
    {
        if (isHovered)
        {
            if (isTimingCorrect)
            {
                HandleSuccessfulAttack();
            }
            else
            {
                ResetAttackState();
            }
        }
    }
    #endregion

    #region Attack Handling Methods
    private void HandleSuccessfulAttack()
    {
        currentHitCount++;

        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.1f)
            {
                point.GetComponent<SpriteRenderer>().color = Color.green;
                break;
            }
        }

        if (currentHitCount >= data.clicksToDestroy)
        {
            DeactivateObstacle();
        }
    }
    private void ResetAttackState()
    {
        if (timingIndicator != null)
        {
            timingIndicator.localPosition = initialTimingIndicatorPos;
        }

        currentHitCount = 0;

        foreach (Transform point in attackPoints)
        {
            point.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    #endregion

    #region Obstacle Management Methods
    private void DeactivateObstacle()
    {
        gameObject.SetActive(false);
    }

    public void ReactivateObstacle(Vector3 newPosition)
    {
        gameObject.SetActive(true);
        transform.position = newPosition;
    }

    public void SlowSpeed()
    {
        currentSpeed *= 0.5f;
    }

    public void ResetSpeed()
    {
        currentSpeed = data.speed;
    }
    #endregion
}
