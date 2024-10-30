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
    public float timingRotationSpeed = 100f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector3 initialTimingIndicatorPos;
    private int currentHitCount = 0;
    private float currentSpeed;
    private float stopDistance = 0.3f;
    private bool isHovered = false;
    private bool isActive = true;
    private bool isTimingCorrect = false;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ObstacleManager.Instance.RegisterObstacle(this);
        currentSpeed = data.speed;
        initialTimingIndicatorPos = timingIndicator.localPosition;
    }

    private void Update()
    {
        if (isActive)
        {
            MoveTowardsPlayer();
            HandleMouseHover();
            HandleHoverEffect();
        }
    }

    #region Movement Methods
    private void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            // transform.position += direction * currentSpeed * Time.deltaTime;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > stopDistance)
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

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            if(!isHovered)
            {
                isHovered = true;
                ObstacleManager.Instance.SlowDownAllObstacles();
            }
        }
        else
        {
            if (isHovered)
            {
                isHovered = false;
                ObstacleManager.Instance.ResetAllObstaclesSpeed();
            }
        }
    }

    private void HandleHoverEffect()
    {
        if (isHovered)
        {
            attackGroup.SetActive(true);
            RotateTimingIndicator();
        }
        else
        {
            attackGroup.SetActive(false);
            ResetAttackState();

        }
    }

    private void RotateTimingIndicator()
    {
        if (timingIndicator != null)
        {
            timingIndicator.RotateAround(transform.position, Vector3.back, timingRotationSpeed * Time.deltaTime);
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
        if (isTimingCorrect && isHovered)
        {
            HandleSuccessfulAttack();
        }
        else if (isHovered)
        {
            ResetAttackState();
        }
    }
    #endregion

    #region Attack Handling Methods
    private void HandleSuccessfulAttack()
    {
        currentHitCount++;
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
        isActive = false;
        gameObject.SetActive(false);
        ObstacleManager.Instance.UnregisterObstacle(this);
    }

    public void ReactivateObstacle(Vector3 newPosition)
    {
        gameObject.SetActive(true);
        isActive = true;

        transform.position = newPosition;
        currentHitCount = 0;
        currentSpeed = data.speed;
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
