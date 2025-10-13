using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ObstacleProjectile : MonoBehaviour
{
    [Header("Movement")]
    private Vector3 target;
    private GameObject player;
    private float speed;
    private ProjectileObstacleController obstacleController;
    
    [Header("Click Destruction")]
    public int clicksToDestroy = 3;
    public Sprite[] damageSprites; // 0타, 1타, 2타 스프라이트
    public Transform[] attackPoints; // 클릭 포인트들
    public Transform timingIndicator; // 타이밍 인디케이터
    public GameObject attackGroup;
    public float timingRotationSpeed = 100f;
    
    [Header("Effects")]
    public GameObject destroyEffectPrefab;
    public GameObject attackEffectPrefab;
    public GameObject bodyAttackEffectPrefab;
    public GameObject smokeEffectPrefab;
    
    private SpriteRenderer spriteRenderer;
    private InputAction clickAction;
    private bool isHovered = false;
    private bool isRotating = true;
    private bool isDestroyed = false;
    private int currentHitCount = 0;
    private List<Transform> clickedPoints = new List<Transform>();
    private Dictionary<Transform, bool> attackPointStates = new Dictionary<Transform, bool>();
    private Vector3 initialTimingIndicatorPos;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (timingIndicator != null)
            initialTimingIndicatorPos = timingIndicator.localPosition;

        // 공격 포인트 초기화
        if (attackPoints != null)
        {
            foreach (Transform point in attackPoints)
            {
                attackPointStates[point] = true;
            }
        }
        
        if (damageSprites != null && damageSprites.Length > 0)
        {
            spriteRenderer.sprite = damageSprites[0];
        }
    }

    public void Initialize(Vector3 targetPos, GameObject playerObject, float moveSpeed, 
                          ProjectileObstacleController obstacleRef)
    {
        target = targetPos;
        player = playerObject;
        speed = moveSpeed;
        obstacleController = obstacleRef;

        var playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.FindActionMap("Gimmick").Enable();
            playerInput.actions.FindActionMap("Player").Disable();

            clickAction = playerInput.actions["Projectile"];
            clickAction.performed += OnClickPerformed;
        }
    }

    void Update()
    {
        if (isDestroyed) return;
        
        // 목표 지점으로 이동
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            DestroyProjectile(false); // 파괴 없이 제거
        }
        
        // 타이밍 인디케이터 회전
        if (isHovered && isRotating && timingIndicator != null)
        {
            timingIndicator.RotateAround(transform.position, Vector3.back, 
                                        timingRotationSpeed * Time.deltaTime);
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnPlayerAttack();
                break;
            }
        }
    }

    private void OnMouseEnter()
    {
        SetHovered(true);
    }

    private void OnMouseExit()
    {
        SetHovered(false);
    }

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered || isDestroyed) return;

        isHovered = hovered;
        HandleHoverEffect();
    }

    private void HandleHoverEffect()
    {
        if (attackGroup == null || isDestroyed) return;
        
        SpriteRenderer renderer = attackGroup.GetComponent<SpriteRenderer>();
        if (renderer == null) return;
        
        renderer.DOKill();
        
        if (isHovered)
        {
            attackGroup.SetActive(true);
            DOTween.To(() => renderer.color.a, 
                    x => {
                        if (renderer != null && attackGroup != null)
                        {
                            renderer.color = new Color(1, 1, 1, x);
                        }
                    }, 
                    1f, 0.5f);
        }
        else
        {
            DOTween.To(() => renderer.color.a, 
                    x => {
                        if (renderer != null && attackGroup != null)
                        {
                            renderer.color = new Color(1, 1, 1, x);
                        }
                    }, 
                    0f, 0.2f)
                .OnComplete(() => { 
                    if (attackGroup != null)
                    {
                        attackGroup.SetActive(false); 
                    }
                });
        }
    }

    private void OnPlayerAttack()
    {
        if (!isHovered || isDestroyed) return;

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

    private bool CheckTiming()
    {
        if (attackPoints == null || timingIndicator == null) return false;
        
        foreach (Transform point in attackPoints)
        {
            float distance = Vector3.Distance(timingIndicator.position, point.position);

            if (distance < 0.3f && !clickedPoints.Contains(point) && attackPointStates[point])
            {
                return true;
            }
        }

        return false;
    }

    private void HandleSuccessfulAttack()
    {
        foreach (Transform point in attackPoints)
        {
            if (Vector3.Distance(timingIndicator.position, point.position) < 0.3f && 
                attackPointStates[point])
            {
                // 오디오 재생
                string audioName = "obstacle_click_" + point.name[point.name.Length - 1];
                AudioManager.Instance.PlaySFX(audioName, 
                                             gameObject.GetComponent<AudioSource>(), 
                                             transform);

                // 흔들림 효과
                transform.DOShakePosition(0.5f, 0.1f);
                
                // 이펙트 생성
                if (attackEffectPrefab != null)
                {
                    GameObject attackEffect = Instantiate(attackEffectPrefab, point.position, 
                                                         Quaternion.identity);
                    attackEffect.GetComponent<ParticleSystem>().Play();
                    Destroy(attackEffect, 0.5f);
                }
                
                if (bodyAttackEffectPrefab != null)
                {
                    GameObject bodyEffect = Instantiate(bodyAttackEffectPrefab, transform.position, 
                                                       Quaternion.identity);
                    bodyEffect.GetComponent<ParticleSystem>().Play();
                    Destroy(bodyEffect, 2f);
                }

                // 포인트 비활성화
                Color color = point.GetComponent<SpriteRenderer>().color;
                color.a = 100f;
                point.GetComponent<SpriteRenderer>().color = color;
                clickedPoints.Add(point);
                attackPointStates[point] = false;
                
                currentHitCount++;
                UpdateSprite();
                break;
            }
        }

        // 파괴 체크
        if (currentHitCount >= clicksToDestroy)
        {
            DestroyProjectile(true);
        }
    }

    private IEnumerator DeactivateAllPointsTemporarily(float delay)
    {
        isRotating = false;

        // 인디케이터 반투명화
        if (timingIndicator != null)
        {
            SpriteRenderer indicatorRenderer = timingIndicator.GetComponent<SpriteRenderer>();
            if (indicatorRenderer != null)
            {
                Color color = indicatorRenderer.color;
                color.a = 0.2f;
                indicatorRenderer.color = color;
            }
        }

        // 모든 포인트 일시 비활성화
        foreach (Transform point in attackPoints)
        {
            attackPointStates[point] = false;
        }

        yield return new WaitForSeconds(delay);

        // 인디케이터 복구
        if (timingIndicator != null)
        {
            SpriteRenderer indicatorRenderer = timingIndicator.GetComponent<SpriteRenderer>();
            if (indicatorRenderer != null)
            {
                Color color = indicatorRenderer.color;
                color.a = 1f;
                indicatorRenderer.color = color;
            }
        }

        // 클릭 안된 포인트만 재활성화
        foreach (Transform point in attackPoints)
        {
            if (!clickedPoints.Contains(point))
            {
                attackPointStates[point] = true;
            }
        }

        isRotating = true;
    }

    private void UpdateSprite()
    {
        if (damageSprites != null && currentHitCount < damageSprites.Length)
        {
            spriteRenderer.sprite = damageSprites[currentHitCount];
        }
    }

    private void DestroyProjectile(bool countAsDestroyed)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        transform.DOKill();
        if (attackGroup != null)
        {
            attackGroup.GetComponent<SpriteRenderer>()?.DOKill();
        }

        if (countAsDestroyed)
        {
            // 파괴 이펙트
            if (smokeEffectPrefab != null)
            {
                GameObject smokeEffect = Instantiate(smokeEffectPrefab, transform.position, 
                                                    Quaternion.identity);
                smokeEffect.transform.SetParent(null);
            }

            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (destroyEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(destroyEffectPrefab, transform.position, 
                                                    Quaternion.identity);
                        Destroy(effect, 2f);
                    }
                    
                    // 장애물 컨트롤러에 알림
                    if (obstacleController != null)
                    {
                        obstacleController.OnOneProjectileDestroyed();
                    }
                    
                    Destroy(gameObject);
                });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (clickAction != null)
        {
            clickAction.performed -= OnClickPerformed;
        }
        
        transform.DOKill();
        if (attackGroup != null)
        {
            SpriteRenderer renderer = attackGroup.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.DOKill();
            }
        }
    }
}