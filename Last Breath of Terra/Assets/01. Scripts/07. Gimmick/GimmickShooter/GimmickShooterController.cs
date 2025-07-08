using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickShooterController : MonoBehaviour
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

        foreach (var gimmickGroup in gimmickGroups)
        {
            gimmickGroup.Initialize(this);
        }
        currentGroup.StartShooter();

    }
    public void ChangeGimmickGroup(int mapID)
    {
        currentGroup.StopShooter();
        currentGroup = gimmickGroups[mapID];
        currentGroup.StartShooter();
        
    }
}
