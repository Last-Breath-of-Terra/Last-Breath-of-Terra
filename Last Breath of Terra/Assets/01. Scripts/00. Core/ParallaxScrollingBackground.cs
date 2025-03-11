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
		background_width = background_rightPoint.position.x - background_leftPoint.position.x;

		float camera_width = camera_leftPoint.position.x - camera_rightPoint.position.x;
		ground_sideSpace = ground_rightPoint.position.x - ground_leftPoint.position.x;
		background_sideSpace = background_leftPoint.position.x - background_rightPoint.position.x - camera_width * 0.5f;
	}

	void Update() {
		SetPosition();
	}

	void SetPosition() {
		float background_xPos = camera_object.transform.position.x + ((camera_object.transform.position.x - ground_leftPoint.position.x) / ground_sideSpace - 0.5f) * background_sideSpace;

		background_xPos = Mathf.Clamp(background_xPos, ground_leftPoint.position.x - (background_width / 2), ground_rightPoint.position.x - (background_width / 2));

        transform.position = new Vector2(background_xPos, transform.position.y);
	}
}