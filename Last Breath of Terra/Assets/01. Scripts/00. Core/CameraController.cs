using UnityEngine;
using Cinemachine;
using DG.Tweening;


public class CameraController : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private CinemachineFramingTransposer _framingTransposer;
    private CinemachineVirtualCamera _virtualCamera;



    public bool isYlockZone;
    void Start()
    {
        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

    }

    void Update()
    {
        UpdateDeadZone();
    }

    private void UpdateDeadZone()
    {
        if (_framingTransposer != null && playerMovement != null)
        {
            if (!isYlockZone)
            {
                if (playerMovement.IsSignificantFall())
                {
                    _framingTransposer.m_DeadZoneHeight = 0f;
                }
                else if (playerMovement.IsJumping())
                {
                    _framingTransposer.m_DeadZoneHeight = 0.7f;
                }
                else
                {
                    _framingTransposer.m_DeadZoneHeight = 0f;
                }
            }
            
        }
    }
    
    public void LockCameraXPosition()
    {
        Debug.Log("Fixing camera X position");
        _framingTransposer.m_DeadZoneWidth = 1f;
    }
    public void UnlockCameraXPosition(){
        Debug.Log("Unlocking camera X position");
        _framingTransposer.m_DeadZoneWidth = 0;
    }
    
    public void LockCameraYPosition()
    {
        isYlockZone = true;
        Debug.Log("Fixing camera Y position");
        _framingTransposer.m_DeadZoneHeight = 1f;
    }
    public void UnlockCameraYPosition(){
        isYlockZone = false;
        Debug.Log("Unlocking camera Y position");
        _framingTransposer.m_DeadZoneHeight = 0;
    }
    public void ChangeCameraLensSize(float targetLensRatio)
    {
        float defaultLensSize = _virtualCamera.m_Lens.OrthographicSize;
        float targetLensSize = defaultLensSize * targetLensRatio;
        DOTween.To(() => defaultLensSize, x => _virtualCamera.m_Lens.OrthographicSize = x, targetLensSize, 1.5f);
    }
    
    
}
