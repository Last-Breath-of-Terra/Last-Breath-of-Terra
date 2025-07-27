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
        if(currentGroup != null)
            currentGroup.StartShooter();

    }
    public void ChangeGimmickGroup(int mapID)
    {
        if (gimmickGroups[mapID] != null)
        {
            PoolManager.Instance.ReturnAll(poolName);
            if (currentGroup != null)
            {
                currentGroup.StopShooter();
            }
            currentGroup = gimmickGroups[mapID];
            currentGroup.StartShooter();
        }
    }
}
