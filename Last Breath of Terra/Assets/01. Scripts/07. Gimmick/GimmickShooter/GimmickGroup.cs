using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickGroup : MonoBehaviour
{
    public int mapID;
    [SerializeField] private List<GimmickShooter> shooters;

    private void OnEnable()
    {
        foreach (var shooter in shooters)
        {
            shooter.StartShooter();
        }
    }

    private void OnDisable()
    {
        foreach (var shooter in shooters)
        {
            shooter.StopShooter();
        }
    }

}