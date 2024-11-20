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
    public StageLifeInfuserSO lifeInfuserSO;
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
    private Dictionary<Transform, bool> attackPointStates = new Dictionary<Transform, bool>();

    private void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        initialTimingIndicatorPos = timingIndicator.localPosition;

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = true;
        }
    }

    private void OnEnable()
    {
        isActive = true;
        ObstacleManager.Instance.RegisterObstacle(this);
        AudioManager.instance.PlayRandomSFX("obstacle_dark_move_", GetComponent<AudioSource>(), transform);
        
        currentHitCount = 0;
        currentSpeed = data.speed;
        clickedPoints.Clear();

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = true;
        }
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
        else
        {
            StartCoroutine(DeactivateAllPointsTemporarily(2f));
        }
    }

    private IEnumerator DeactivateAllPointsTemporarily(float delay)
    {
        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = false;
        }

        yield return new WaitForSeconds(delay);

        foreach (Transform point in attackPoints)
        {
            if (!clickedPoints.Contains(point))
            {
                attackPointStates[point] = true;
            }
        }
    }
    #endregion

    #region Attack Handling Methods
    private void HandleSuccessfulAttack()
    {
        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.1f && attackPointStates[point])
            {
                string audioName = "obstacle_click_" + point.name[point.name.Length - 1];
                Debug.Log("audioName : " + audioName);
                AudioManager.instance.PlaySFX(audioName, GetComponent<AudioSource>(), transform);
                Color color = point.GetComponent<SpriteRenderer>().color;
                color.a = 100f;
                point.GetComponent<SpriteRenderer>().color = color;
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

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = true;
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

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player"))
        {
            GameManager.Instance.miniMapManager.ForceCloseMap();

            lifeInfuserSO.StopInfusion();
            player.GetComponent<PlayerController>().data.hp -= 10f;

            float playerFacingDirection = player.transform.localScale.x;
            Vector2 knockbackDirection = playerFacingDirection > 0 ? Vector2.left : Vector2.right;
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            playerRb.AddForce(knockbackDirection * 2f, ForceMode2D.Impulse);
            Invoke(nameof(ReactivatePlayerMovement), 1f);

            DeactivateObstacle();
        }
    }

    private void ReactivatePlayerMovement()
    {
        player.GetComponent<PlayerController>().SetCanMove(true);
    }
}
