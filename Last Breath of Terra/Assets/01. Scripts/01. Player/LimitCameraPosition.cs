using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class LimitCameraPosition : MonoBehaviour
{
    public float changelensRatio;
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineFramingTransposer framingTransposer;
    private CameraController cameraController;

    void Start()
    {
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController not found!");
        }
        if (virtualCamera != null)
        {
            framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.CompareTag("LockXPositon"))
            {
                LockCameraXPosition();
            }
            else if (gameObject.CompareTag("LockYPositon"))
            {
                LockCameraYPosition();
            }
            else if (gameObject.CompareTag("ChangeLensSize"))
            {
                ChangeCameraLensSize(changelensRatio);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.CompareTag("LockXPositon"))
            {
                UnlockCameraXPosition();
            }
            else if (gameObject.CompareTag("LockYPositon"))
            {
                UnlockCameraYPosition();
            }
            else if (gameObject.CompareTag("ChangeLensSize"))
            {
                ChangeCameraLensSize(1f / changelensRatio);
            }
        }
    }
    
    private void LockCameraXPosition()
    {
        Debug.Log("Fixing camera X position");
        framingTransposer.m_DeadZoneWidth = 1f;
    }
    private void UnlockCameraXPosition(){
        Debug.Log("Unlocking camera X position");
        framingTransposer.m_DeadZoneWidth = 0;
    }
    
    private void LockCameraYPosition()
    {
        cameraController.isYlockZone = true;
        Debug.Log("Fixing camera Y position");
        framingTransposer.m_DeadZoneHeight = 1f;
    }
    private void UnlockCameraYPosition(){
        cameraController.isYlockZone = false;
        Debug.Log("Unlocking camera Y position");
        framingTransposer.m_DeadZoneHeight = 0;
    }
    private void ChangeCameraLensSize(float targetLensRatio)
    {
        float defaultLensSize = virtualCamera.m_Lens.OrthographicSize;
        float targetLensSize = defaultLensSize * targetLensRatio;
        DOTween.To(() => defaultLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, targetLensSize, 1.5f);
    }
}
