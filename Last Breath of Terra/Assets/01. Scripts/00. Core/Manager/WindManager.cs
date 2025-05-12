using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class WindManager : Singleton<WindManager>
{
    public enum WindType
    {
        Fast,
        Slow,
        Up
    }

    
    public WindSO windSO;
    private PlayerController player;

    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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
            case WindType.Up:
                Transform playerTransform = player.transform;
                playerTransform.DOMoveY(playerTransform.position.y + windSO.liftHeight, windSO.liftDuration);
                break;
            default:
                drag = 1f;
                break;
        }

        player.SpeedChangeRate = drag;
    }

    public void RemoveWindEffect(WindType windType)
    {
        switch (windType)
        {
            
            case WindType.Up:
                Transform playerTransform = player.transform;
                playerTransform.DOMoveY(playerTransform.position.y - windSO.liftHeight, windSO.liftDuration);
                break;
            default:
                player.SpeedChangeRate = 1f;
                break;
                
        }
    }
}
