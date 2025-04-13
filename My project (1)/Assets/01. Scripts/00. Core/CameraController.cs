using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public PlayerController playerController;
    private CinemachineFramingTransposer framingTransposer;

    public bool isYlockZone;
    void Start()
    {
        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    void Update()
    {
        UpdateDeadZone();
    }

    private void UpdateDeadZone()
    {
        if (framingTransposer != null && playerController != null)
        {
            if (!isYlockZone)
            {
                if (playerController.IsSignificantFall())
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
                else if (playerController.isJumping)
                {
                    framingTransposer.m_DeadZoneHeight = 0.7f;
                }
                else
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
            }
            
        }
    }
    
    
}
