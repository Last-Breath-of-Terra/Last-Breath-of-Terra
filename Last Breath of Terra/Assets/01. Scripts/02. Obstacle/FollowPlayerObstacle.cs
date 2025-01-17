using System;
using UnityEngine;

/// <summary>
/// 플레이어를 따라가는 장애물
/// </summary>
public class FollowPlayerObstacle : Obstacle
{
    private float speed = 1.5f;

    private void Update()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (GameManager.Instance.playerTr.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
    }

}
