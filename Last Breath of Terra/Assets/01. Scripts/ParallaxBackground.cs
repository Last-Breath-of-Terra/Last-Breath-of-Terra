using UnityEngine;
using System.Collections.Generic;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;           // 카메라의 Transform
    public Vector2 parallaxEffect;              // X, Y 축의 패럴랙스 효과 비율
    public List<Sprite> backgroundSprites;      // 배경 이미지 리스트
    public int mapID = 0;                       // 현재 맵 ID (0~10)

    private Vector3 lastCameraPosition;
    private SpriteRenderer spriteRenderer;
    private int currentMapID = -1;              // 현재 적용된 맵 ID (변경 감지용)

    void Start()
    {
        // 초기 카메라 위치 저장
        lastCameraPosition = cameraTransform.position;

        // SpriteRenderer 컴포넌트 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 초기 배경 설정
        UpdateBackgroundSprite();
    }

    void Update()
    {
        // 카메라의 이동량 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 배경 이동
        transform.position += new Vector3(deltaMovement.x * parallaxEffect.x, deltaMovement.y * parallaxEffect.y, 0);

        // 현재 카메라 위치 저장
        lastCameraPosition = cameraTransform.position;

        // MapID 변경 시 배경 업데이트
        if (currentMapID != mapID)
        {
            UpdateBackgroundSprite();
        }
    }

    // 배경 스프라이트 업데이트
    private void UpdateBackgroundSprite()
    {
        if (mapID >= 0 && mapID < backgroundSprites.Count)
        {
            spriteRenderer.sprite = backgroundSprites[mapID];
            currentMapID = mapID; // 현재 맵 ID 업데이트
        }
        else
        {
            Debug.LogWarning("MapID out of range: " + mapID);
        }
    }
}