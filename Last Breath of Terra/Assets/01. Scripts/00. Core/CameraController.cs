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
                // 지형에서는 카메라 고정 해제 (점프해서 올라가면 따라가야해서)
                if (playerController.isGrounded)
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
                else if (playerController.IsSignificantFall())
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
                // 지형에서 하강일때, 카메라가 따라가도록 설정
                else if (playerController.isJumping)
                {
                    framingTransposer.m_DeadZoneHeight = 0.7f;
                }
                // 점프중 일때는 카메라 고정
                else
                {
                    framingTransposer.m_DeadZoneHeight = 0f;
                }
            }
            
        }
    }
    
    
}
