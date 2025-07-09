using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class WindManager : Singleton<WindManager>
{
    
    public enum WindDirection
    {
        Left = -1,
        Right = 1
    }
    public enum WindType
    {
        Fast,
        Slow
    }

    
    public WindSO windSO;
    private PlayerMovement player;

    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void ApplyWindEffect(WindType windType)
    {
        float drag = 1f;
        switch (windType)
        {
            
            case WindType.Fast:
                drag = windSO.fastRate;
                break;
            case WindType.Slow:
                drag = windSO.slowRate;
                break;
            default:
                drag = 1f;
                break;
        }

        player.SpeedChangeRate = drag;
    }
    
    public void RemoveWindEffect(WindType windType)
    {
        player.SpeedChangeRate = 1f;

    }
}
