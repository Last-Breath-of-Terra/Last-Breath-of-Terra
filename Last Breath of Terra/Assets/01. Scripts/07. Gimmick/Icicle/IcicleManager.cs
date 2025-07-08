using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleManager : Singleton<IcicleManager>
{
    public GameObject IciclePrefab;
    public string poolName;
    public float delayTimer;
    void Start()
    {
        PoolManager.Instance.CreatePool(poolName, IciclePrefab, gameObject.transform);
    }

}
