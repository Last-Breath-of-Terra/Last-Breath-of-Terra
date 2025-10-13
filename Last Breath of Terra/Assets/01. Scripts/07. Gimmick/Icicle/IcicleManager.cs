using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleManager : Singleton<IcicleManager>
{
    public GameObject IciclePrefab;
    public string poolName;

    void Start()
    {
        poolName = IciclePrefab.name;
        PoolManager.Instance.CreatePool(poolName, IciclePrefab, gameObject.transform);
    }

}
