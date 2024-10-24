using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 적의 기능을 담당하고 있는 클래스
/// </summary>

public class Enemy : MonoBehaviour
{
    public EnemySO enemyData;
    public Transform[] attackPoints;
    public Transform timingIndicator;
    public GameObject attackGroup;
    public float timingRotationSpeed = 100f;

    private Rigidbody2D rb;
    private Transform player;
    private Vector3 initialTimingIndicatorPos;
    private int currentHitCount = 0;
    private float currentSpeed;
    private float stopDistance = 1.5f;
    private bool isHovered = false;
    private bool isActive = true;
    private bool isTimingCorrect = false;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        EnemyManager.Instance.RegisterEnemy(this);
        currentSpeed = enemyData.speed;
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
                EnemyManager.Instance.SlowDownAllEnemies();
            }
        }
        else
        {
            if (isHovered)
            {
                isHovered = false;
                EnemyManager.Instance.ResetAllEnemiesSpeed();
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
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.5f)
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
        if (currentHitCount >= enemyData.clicksToDestroy)
        {
            DeactivateEnemy();
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

    #region Enemy Management Methods
    private void DeactivateEnemy()
    {
        isActive = false;
        gameObject.SetActive(false);
        EnemyManager.Instance.UnregisterEnemy(this);
    }

    public void ReactivateEnemy(Vector3 newPosition)
    {
        gameObject.SetActive(true);
        isActive = true;

        transform.position = newPosition;
        currentHitCount = 0;
        currentSpeed = enemyData.speed;
    }

    public void SlowSpeed()
    {
        currentSpeed *= 0.5f;
    }

    public void ResetSpeed()
    {
        currentSpeed = enemyData.speed;
    }
    #endregion
}
