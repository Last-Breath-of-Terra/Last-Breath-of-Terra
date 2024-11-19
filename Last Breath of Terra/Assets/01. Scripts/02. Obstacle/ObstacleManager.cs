using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장애물의 오브젝트를 관리하는 Manager 클래스
/// 장애물을 배열에 추가하거나 제거하는 역할
/// </summary>

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance;

    private List<Obstacle> allObstacles = new List<Obstacle>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterObstacle(Obstacle obstacle)
    {
        if (!allObstacles.Contains(obstacle))
        {
            allObstacles.Add(obstacle);
        }
    }

    public void UnregisterObstacle(Obstacle obstacle)
    {
        if (allObstacles.Contains(obstacle))
        {
            allObstacles.Remove(obstacle);
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
