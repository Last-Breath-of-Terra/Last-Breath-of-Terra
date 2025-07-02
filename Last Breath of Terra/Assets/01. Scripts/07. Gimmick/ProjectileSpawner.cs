using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileSpawner : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject player;
    public ProjectileObstacleController obstacle;

    [Header("Detail Settings")]
    public int projectileCount = 3;
    public float spawnInterval = 3f;

    [Header("Speed Settings")]
    public float projectileSpeedMin = 5f;
    public float projectileSpeedMax = 10f;

    private bool playerInside = false;
    private Coroutine spawnCoroutine;
    private BoxCollider2D spawnArea;

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!obstacle.IsDestroyed())
            {
                spawnCoroutine = StartCoroutine(SpawnProjectiles());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }

    IEnumerator SpawnProjectiles()
    {
        while (true)
        {
            // 장애물이 파괴되면 멈춘다
            if (obstacle.IsDestroyed())
            {
                spawnCoroutine = null;
                yield break;
            }

            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                GameObject p = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

                float facingDirection = Mathf.Sign(player.transform.localScale.x);
                Vector3 targetPos = player.transform.position + new Vector3(facingDirection * 5f, 0f, 0f);
                targetPos.z = 0f;

                float speed = Random.Range(projectileSpeedMin, projectileSpeedMax);
                p.GetComponent<ObstacleProjectile>().Initialize(targetPos, player, speed, obstacle);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 center = (Vector2)transform.position + spawnArea.offset;
        Vector2 size = spawnArea.size;

        float randomX = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
        float randomY = Random.Range(center.y, center.y + size.y / 2f);

        return new Vector2(randomX, randomY);
    }

    public void TryResumeSpawn()
    {
        if (playerInside && spawnCoroutine == null && !obstacle.IsDestroyed())
        {
            spawnCoroutine = StartCoroutine(SpawnProjectiles());
        }
    }
}