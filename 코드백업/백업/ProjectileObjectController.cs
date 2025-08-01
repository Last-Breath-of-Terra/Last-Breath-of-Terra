using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileObstacleController : MonoBehaviour
{
    public int requiredDestroyedCount = 3;
    public float respawnTime = 5f;
    [SerializeField] private float destroyDelay = 0.5f;

    private int currentDestroyedCount = 0;
    private bool isDestroyed = false;

    public GameObject obstacleVisual;
    private Collider2D obstacleCollider;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] stageSprites;

    private void Awake()
    {
        obstacleCollider = GetComponent<Collider2D>();
    }

    public void OnOneProjectileDestroyed()
    {
        if (isDestroyed) return;

        currentDestroyedCount++;

        // 스프라이트 교체
        if (spriteRenderer != null && currentDestroyedCount <= stageSprites.Length)
        {
            spriteRenderer.sprite = stageSprites[Mathf.Min(currentDestroyedCount, stageSprites.Length - 1)];
        }

        if (currentDestroyedCount >= requiredDestroyedCount)
        {
            StartCoroutine(DestroyObstacleWithDelay());
        }
    }

    private IEnumerator DestroyObstacleWithDelay()
    {
        isDestroyed = true;

        yield return new WaitForSeconds(destroyDelay);

        // 오브젝트 모습 비활성화
        if (obstacleVisual != null)
            obstacleVisual.SetActive(false);

        if (obstacleCollider != null)
            obstacleCollider.enabled = false;

        var playerInput = GameManager.Instance.playerTr.gameObject.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.FindActionMap("Gimmick").Disable();
            playerInput.actions.FindActionMap("Player").Enable();
        }

        // 재생성 루틴
        StartCoroutine(RespawnObstacleAfterDelay());
    }

    IEnumerator RespawnObstacleAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        isDestroyed = false;
        currentDestroyedCount = 0;

        if (obstacleVisual != null)
            obstacleVisual.SetActive(true);

        if (obstacleCollider != null)
            obstacleCollider.enabled = true;

        // 스프라이트 초기화
        if (spriteRenderer != null && stageSprites.Length > 0)
            spriteRenderer.sprite = stageSprites[0];

        var spawner = GetComponentInParent<ProjectileSpawner>();
        spawner.TryResumeSpawn();
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}