using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 오브젝트를 관리하는 Manager 클래스
/// 적을 배열에 추가하거나 제거하는 역할
/// </summary>

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private List<Enemy> allEnemies = new List<Enemy>();

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

    public void RegisterEnemy(Enemy enemy)
    {
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (allEnemies.Contains(enemy))
        {
            allEnemies.Remove(enemy);
        }
    }

    public void SlowDownAllEnemies()
    {
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.isActiveAndEnabled)
            {
                enemy.SlowSpeed();
            }
        }
    }

    public void ResetAllEnemiesSpeed()
    {
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy.isActiveAndEnabled)
            {
                enemy.ResetSpeed();
            }
        }
    }
}
