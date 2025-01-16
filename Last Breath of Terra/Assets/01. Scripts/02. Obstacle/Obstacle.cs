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
    public LifeInfuserSO lifeInfuserSO;
    public Sprite[] obstacleSprites;
    public Transform[] attackPoints;
    public Transform timingIndicator;
    public Transform targetPoint;
    public GameObject attackGroup;
    public GameObject destroyEffectPrefab;

    protected bool isHovered = false;
    protected bool isRotating = true;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D _rb;
    private Vector3 initialTimingIndicatorPos;
    private int currentHitCount = 0;
    private bool isActive = true;
    private float currentSpeed;
    private List<Transform> clickedPoints = new List<Transform>();
    private Dictionary<Transform, bool> attackPointStates = new Dictionary<Transform, bool>();

    private void Awake()
    {
        _rb = this.GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialTimingIndicatorPos = timingIndicator.localPosition;

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = true;
        }

        if (obstacleSprites != null && obstacleSprites.Length > 0)
        {
            spriteRenderer.sprite = obstacleSprites[0];
        }
    }

    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        isActive = true;

        if (GameManager.ScenesManager.GetCurrentSceneType() == SCENE_TYPE.Tutorial)
        {
            GameManager.Instance._obstacleManager.RegisterObstacle(this);
            AudioManager.instance.PlayRandomSFX("obstacle_dark_move_", gameObject.GetComponent<AudioSource>(), transform);
        }
        
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
    }

    private void Update()
    {
        if (isActive)
        {
            MoveTowardsTarget();
        }

        if (isHovered && isRotating)
        {
            RotateTimingIndicator();
        }
    }

    #region Movement Methods
    private void MoveTowardsTarget()
    {
        if (targetPoint == null) return;

        Vector3 direction = (targetPoint.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
    #endregion

    #region Hover and Attack Methods
    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered) return;

        isHovered = hovered;

        if (isHovered)
        {
            GameManager.Instance._obstacleManager.SlowDownAllObstacles();
        }
        else
        {
            GameManager.Instance._obstacleManager.ResetAllObstaclesSpeed();
        }

        HandleHoverEffect();
    }

    protected void HandleHoverEffect()
    {
        attackGroup.SetActive(isHovered);
    }

    public void RotateTimingIndicator()
    {
        if (timingIndicator != null)
        {
            timingIndicator.RotateAround(transform.position, Vector3.back, data.timingRotationSpeed * Time.deltaTime);
            //CheckTiming();
        }
    }

    private bool CheckTiming()
    {
        foreach (Transform point in attackPoints)
        {
            float distance = Vector3.Distance(timingIndicator.position, point.position);

            if (distance < 0.3f && !clickedPoints.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    public void OnPlayerAttack()
    {
        if (!isHovered) return;

        bool timingMatched = CheckTiming();

        if (timingMatched)
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
        isRotating = false;

        SpriteRenderer indicatorRenderer = timingIndicator.GetComponent<SpriteRenderer>();
        if (indicatorRenderer != null)
        {
            Color color = indicatorRenderer.color;
            color.a = 0.2f;
            indicatorRenderer.color = color;
        }

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = false;
        }

        yield return new WaitForSeconds(delay);

        if (indicatorRenderer != null)
        {
            Color color = indicatorRenderer.color;
            color.a = 1f;
            indicatorRenderer.color = color;
        }

        foreach (Transform point in attackPoints)
        {
            if (!clickedPoints.Contains(point))
            {
                attackPointStates[point] = true;
            }
        }

        isRotating = true;
    }
    #endregion

    #region Attack Handling Methods
    private void HandleSuccessfulAttack()
    {
        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.3f && attackPointStates[point])
            {
                string audioName = "obstacle_click_" + point.name[point.name.Length - 1];
                AudioManager.instance.PlaySFX(audioName, gameObject.GetComponent<AudioSource>(), transform);

                Color color = point.GetComponent<SpriteRenderer>().color;
                color.a = 100f;
                point.GetComponent<SpriteRenderer>().color = color;
                clickedPoints.Add(point);
                currentHitCount++;
                UpdateObstacleSprite();
                break;
            }
        }

        if (currentHitCount >= data.clicksToDestroy)
        {
            Invoke("DeactivateObstacle", 0.1f);
            //DeactivateObstacle();
        }
    }

    private void UpdateObstacleSprite()
    {
        if (obstacleSprites != null && currentHitCount < obstacleSprites.Length)
        {
            spriteRenderer.sprite = obstacleSprites[currentHitCount];
        }
    }

    private void ResetAttackState()
    {
        timingIndicator.localPosition = initialTimingIndicatorPos;
        currentHitCount = 0;

        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = true;

            SpriteRenderer pointRenderer = point.GetComponent<SpriteRenderer>();
            if (pointRenderer != null)
            {
                Color color = pointRenderer.color;
                color.a = 0f;
                pointRenderer.color = color;
            }
        }

        if (obstacleSprites != null && obstacleSprites.Length > 0)
        {
            spriteRenderer.sprite = obstacleSprites[0];
        }

        clickedPoints.Clear();
    }
    #endregion

    #region Obstacle Management Methods
    protected virtual void DeactivateObstacle()
    {
        GameObject effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 2f);

        GameManager.Instance._obstacleManager.ReturnObstacle(this);
    }

    public void ReactivateObstacle(Vector3 newPosition)
    {
        transform.position = newPosition;
        ResetAttackState();
        gameObject.SetActive(true);
    }

    public void SetSpeedToZero()
    {
        currentSpeed = 0f;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger)
        {
            isHovered = true;
            HandleHoverEffect();
            GameManager.Instance._obstacleManager.SlowDownAllObstacles();
        }

        if (collision.CompareTag("InfuserObject"))
        {
            DeactivateObstacle();
        }

        if (collision.transform.CompareTag("Player"))
        {
            lifeInfuserSO.StopInfusion(collision.GetComponent<AudioSource>());

            GameManager.Instance._ui.miniMapManager.ForceCloseMap();

            float playerFacingDirection = GameManager.Instance.playerTr.localScale.x;
            Vector2 knockbackDirection = playerFacingDirection > 0 ? Vector2.left : Vector2.right;
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            AudioManager.instance.PlayRandomSFX("knockback_", collision.GetComponent<AudioSource>(), transform);
            playerRb.AddForce(knockbackDirection * 2f, ForceMode2D.Impulse);
            Invoke(nameof(ReactivatePlayerMovement), 1f);

            DeactivateObstacle();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isTrigger)
        {
            isHovered = false;
            HandleHoverEffect();
            GameManager.Instance._obstacleManager.ResetAllObstaclesSpeed();
        }
    }

    private void ReactivatePlayerMovement()
    {
        GameManager.Instance.playerTr.GetComponent<PlayerController>().SetCanMove(true);
    }
}
