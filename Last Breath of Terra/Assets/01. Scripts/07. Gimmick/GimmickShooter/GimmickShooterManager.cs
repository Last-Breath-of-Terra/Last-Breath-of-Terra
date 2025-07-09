using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickShooterManager : Singleton<GimmickShooterManager>
{
    
    public GameObject obstaclePrefab;
    public string poolName = "GimmickShooter";
    public Vector2 respawnTimeRange = new Vector2(1f, 3f);
    public float shootForce = 10f;
    [SerializeField] private GimmickGroup[] gimmickGroups;
    private GimmickGroup currentGroup;

    void Start()
    {
        PoolManager.Instance.CreatePool(poolName, obstaclePrefab, gameObject.transform);
        currentGroup = gimmickGroups[0];
        currentGroup.StartShooter();

    }
    public void ChangeGimmickGroup(int mapID)
    {
        PoolManager.Instance.ReturnAll(GimmickShooterManager.Instance.poolName);
        currentGroup.StopShooter();
        currentGroup = gimmickGroups[mapID];
        currentGroup.StartShooter();
        
    }
}
