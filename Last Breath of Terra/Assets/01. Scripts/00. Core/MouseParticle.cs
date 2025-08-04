using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseParticle : MonoBehaviour
{
    public Camera uiCamera;
    public float zOffset = 5f; // 카메라와 파티클 간 거리 조절용

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        // z에 Plane Distance와 비슷한 값 넣기 (또는 카메라-파티클 거리)
        mouseScreenPos.z = zOffset;

        Vector3 worldPos = uiCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;

        transform.position = worldPos;
    }
}