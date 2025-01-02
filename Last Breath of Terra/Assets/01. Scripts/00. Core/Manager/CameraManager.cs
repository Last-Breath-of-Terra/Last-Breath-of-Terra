using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FixCameraXPosition();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UnlockCameraXPosition();
    }

    private void FixCameraXPosition()
    {
        Debug.Log("Fixing camera X position");
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 1f;
    }
    private void UnlockCameraXPosition(){
        Debug.Log("Unlocking camera X position");
        virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0;
    }

    private void ChangeCameraLensSize()
    {
        
    }
}
