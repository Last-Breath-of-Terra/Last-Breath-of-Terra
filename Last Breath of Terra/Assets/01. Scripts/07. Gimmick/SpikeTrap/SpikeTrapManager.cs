using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrapManager : Singleton<SpikeTrapManager>
{
    
    public GameObject obstaclePrefab;
    public string poolName = "SpikeTrap";
    
    public float spikeActiveTime;
    public float cooldownTime;
    private GimmickGroup currentGroup;

    void Start()
    {
        PoolManager.Instance.CreatePool(poolName, obstaclePrefab, gameObject.transform);
    }
}
