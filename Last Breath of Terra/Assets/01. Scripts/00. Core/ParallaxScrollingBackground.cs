using UnityEngine;

public sealed class ParallaxScrollingBackground : MonoBehaviour
{
	[SerializeField] GameObject camera_object = null;

	[SerializeField] Transform background_leftPoint = null, background_rightPoint = null;
	[SerializeField] Transform ground_leftPoint = null, ground_rightPoint = null;
	[SerializeField] Transform camera_leftPoint = null, camera_rightPoint = null;

	float ground_sideSpace = 0f, background_sideSpace = 0f;
	float background_width = 0f;

	void Start() {
        // (오른쪽 - 왼쪽) 순으로 해서 양수 폭을 구한다.
        background_width = background_rightPoint.position.x - background_leftPoint.position.x;

        float camera_width = camera_rightPoint.position.x - camera_leftPoint.position.x;
        ground_sideSpace = ground_rightPoint.position.x - ground_leftPoint.position.x;

        // 예: "배경이 이동할 수 있는 범위" - "카메라 폭의 절반" 정도를 빼주고 싶다면
        background_sideSpace = (background_rightPoint.position.x - background_leftPoint.position.x) 
                               - (camera_width * 0.5f);
    }

    void Update() {
        SetPosition();
    }

	void SetPosition() {

		float ratio = (camera_object.transform.position.x - ground_leftPoint.position.x) / ground_sideSpace;
		float offset = (ratio - 0.5f) * background_sideSpace;
		float background_xPos = camera_object.transform.position.x + offset;

		// 배경의 피벗이 중앙이라면, Clamp 시에 절반 너비를 고려합니다.
		background_xPos = Mathf.Clamp(
			background_xPos,
			ground_leftPoint.position.x + (background_width * 0.5f),
			ground_rightPoint.position.x - (background_width * 0.5f)
		);

		transform.position = new Vector3(background_xPos, transform.position.y, transform.position.z);
	}


	// void Start() {
	// 	background_width = background_rightPoint.position.x - background_leftPoint.position.x;

	// 	float camera_width = camera_leftPoint.position.x - camera_rightPoint.position.x;
	// 	ground_sideSpace = ground_rightPoint.position.x - ground_leftPoint.position.x;
	// 	background_sideSpace = background_leftPoint.position.x - background_rightPoint.position.x - camera_width * 0.5f;
	// }

	// void Update() {
	// 	SetPosition();
	// }

	// void SetPosition() {
	// 	float background_xPos = camera_object.transform.position.x + ((camera_object.transform.position.x - ground_leftPoint.position.x) / ground_sideSpace - 0.5f) * background_sideSpace;

	// 	background_xPos = Mathf.Clamp(background_xPos, ground_leftPoint.position.x - (background_width / 2), ground_rightPoint.position.x - (background_width / 2));

    //     transform.position = new Vector2(background_xPos, transform.position.y);
	// }
}