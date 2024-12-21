using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public PlayerController playerController;
    private CinemachineFramingTransposer framingTransposer;

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
            if (playerController.isGrounded)
            {
                framingTransposer.m_DeadZoneHeight = 0f;
            }
            else
            {
                framingTransposer.m_DeadZoneHeight = 0.7f;
            }
        }
    }
}
