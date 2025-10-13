using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public PlayerMovement playerMovement;
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
        if (framingTransposer != null && playerMovement != null)
        {
            if (!isYlockZone)
            {
                if (playerMovement.IsSignificantFall())
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
                else if (playerMovement.IsJumping())
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
