using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickShooterManager : Singleton<GimmickShooterManager>
{
    public GameObject obstaclePrefab;
    public string poolName;
    public Vector2 respawnTimeRange = new Vector2(1f, 3f);
    public float shootForce = 10f;

    void Start()
    {
        PoolManager.Instance.CreatePool(poolName, obstaclePrefab, gameObject.transform);
    }
}