using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라가 플레이어를 따라가도록 하는 스크립트
/// (카메라가 이동할 수 있는 최소값, 최대값 범위 지정)
/// </summary>

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 3;
    public Vector2 offset;
    public float limitMinX, limitMaxX, limitMinY, limitMaxY;

    private float halfWidth, halfHeight;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        halfWidth = mainCamera.aspect * mainCamera.orthographicSize;
        halfHeight = mainCamera.orthographicSize;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x + offset.x, limitMinX + halfWidth, limitMaxX - halfWidth),
            Mathf.Clamp(target.position.y + offset.y, limitMinY + halfHeight, limitMaxY - halfHeight),
            -10);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }
}
