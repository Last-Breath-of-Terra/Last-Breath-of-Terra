using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class LimitCameraPosition : MonoBehaviour
{
    public float changelensRatio;
    private CameraController _cameraController;

    void Start()
    {
        _cameraController = FindObjectOfType<CameraController>();
        if (_cameraController == null)
        {
            Debug.LogError("CameraController not found!");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.CompareTag("LockXPositon"))
            {
                _cameraController.LockCameraXPosition();
            }
            else if (gameObject.CompareTag("LockYPositon"))
            {
                _cameraController.LockCameraYPosition();
            }
            else if (gameObject.CompareTag("ChangeLensSize"))
            {
                _cameraController.ChangeCameraLensSize(changelensRatio);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.CompareTag("LockXPositon"))
            {
                _cameraController.UnlockCameraXPosition();
            }
            else if (gameObject.CompareTag("LockYPositon"))
            {
                _cameraController.UnlockCameraYPosition();
            }
            else if (gameObject.CompareTag("ChangeLensSize"))
            {
                _cameraController.ChangeCameraLensSize(1f / changelensRatio);
            }
        }
    }
    
    
}
