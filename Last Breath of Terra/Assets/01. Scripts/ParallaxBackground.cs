using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;  // 카메라의 Transform
    public Vector2 parallaxEffect;     // X, Y 축의 패럴랙스 효과 비율
    private Vector3 lastCameraPosition;

    void Start()
    {
        // 초기 카메라 위치 저장
        lastCameraPosition = cameraTransform.position;
    }

    void Update()
    {
        // 카메라의 이동량 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 배경 이동
        transform.position += new Vector3(deltaMovement.x * parallaxEffect.x, deltaMovement.y * parallaxEffect.y, 0);

        // 현재 카메라 위치 저장
        lastCameraPosition = cameraTransform.position;
    }
}
