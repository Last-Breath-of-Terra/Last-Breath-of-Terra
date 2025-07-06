using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileObstacleController : MonoBehaviour
{
    public int requiredDestroyedCount = 3;
    public float respawnTime = 5f;
    private int currentDestroyedCount = 0;
    private bool isDestroyed = false;

    public GameObject obstacleVisual;
    private Collider2D obstacleCollider;

    private void Awake()
    {
        obstacleCollider = GetComponent<Collider2D>();
    }

    public void OnOneProjectileDestroyed()
    {
        if (isDestroyed) return;

        currentDestroyedCount++;
        if (currentDestroyedCount >= requiredDestroyedCount)
        {
            DestroyObstacle();
        }
    }

    private void DestroyObstacle()
    {
        isDestroyed = true;

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

        var spawner = GetComponentInParent<ProjectileSpawner>();
        spawner.TryResumeSpawn();
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}