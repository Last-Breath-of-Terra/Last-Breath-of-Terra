using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickGroup : MonoBehaviour
{
    public int mapID;
    [SerializeField] private List<GimmickShooter> shooters;

    public void Initialize(GimmickShooterController controller)
    {
        foreach (var shooter in shooters)
        {
            shooter.Initialize(controller);
        }
    }

    public void StartShooter()
    {
        foreach (var shooter in shooters)
        {
            shooter.StartShooter();
        }
    }

    public void StopShooter()
    {
        foreach (var shooter in shooters)
        {
            shooter.StopShooter();
        }
    }
}