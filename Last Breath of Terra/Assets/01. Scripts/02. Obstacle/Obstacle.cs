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
    private List<Transform> clickedPoints = new List<Transform>();

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
        clickedPoints.Clear();
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
    }

    private void RotateTimingIndicator()
    {
        if (timingIndicator != null)
        {
            timingIndicator.RotateAround(transform.position, Vector3.back, data.timingRotationSpeed * Time.deltaTime);
            CheckTiming();
        }
    }

    private bool CheckTiming()
    {
        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.1f && !clickedPoints.Contains(point))
            {
                return true;
            }
        }
        return false;
    }

    public void OnPlayerAttack()
    {
        if (isHovered && CheckTiming())
        {
            HandleSuccessfulAttack();
        }
    }
    #endregion

    #region Attack Handling Methods
    private void HandleSuccessfulAttack()
    {
        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.1f && !clickedPoints.Contains(point))
            {
                point.GetComponent<SpriteRenderer>().color = Color.green;
                clickedPoints.Add(point);
                currentHitCount++;
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
