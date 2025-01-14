using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장애물의 오브젝트를 관리하는 Manager 클래스
/// 장애물을 배열에 추가하거나 제거하는 역할
/// </summary>

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int poolSize = 10;

    private List<Obstacle> allObstacles = new List<Obstacle>();
    private Transform obstacleParent;

    private void Start()
    {
        GameObject parentObject = new GameObject("Obstacles");
        obstacleParent = parentObject.transform;

        InitializeObstaclePool();
    }

    private void InitializeObstaclePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obstacleObj = Instantiate(obstaclePrefab);
            obstacleObj.transform.SetParent(obstacleParent);
            
            obstacleObj.SetActive(false); 
            Obstacle obstacle = obstacleObj.GetComponent<Obstacle>();

            RegisterObstacle(obstacle);
        }
    }

    public Obstacle GetObstacle()
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (!obstacle.gameObject.activeInHierarchy)
            {
                obstacle.gameObject.SetActive(true);
                return obstacle;
            }
        }

        Debug.LogWarning("모든 장애물이 사용 중입니다!");
        return null;
    }

    public void RegisterObstacle(Obstacle obstacle)
    {
        if (!allObstacles.Contains(obstacle))
        {
            allObstacles.Add(obstacle);
        }
    }

    public void ReturnObstacle(Obstacle obstacle)
    {
        obstacle.gameObject.SetActive(false);
    }

    public void StopAllObstacles()
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle.isActiveAndEnabled)
            {
                obstacle.SetSpeedToZero();
            }
        }
    }

    public void SlowDownAllObstacles()
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle.isActiveAndEnabled)
            {
                obstacle.SlowSpeed();
            }
        }
    }

    public void ResetAllObstaclesSpeed()
    {
        foreach (Obstacle obstacle in allObstacles)
        {
            if (obstacle.isActiveAndEnabled)
            {
                obstacle.ResetSpeed();
            }
        }
    }
}
