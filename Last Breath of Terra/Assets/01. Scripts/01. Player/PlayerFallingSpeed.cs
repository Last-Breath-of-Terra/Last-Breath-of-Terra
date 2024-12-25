using UnityEngine;

public class PlayerFallingSpeed : MonoBehaviour
{
    public float maxFallSpeed = 10f;  // 최대 낙하 속도
    public float fallAcceleration = 2f;  // 낙하 가속도
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 낙하 가속도 적용 및 최고 속도 제한
        if (rb.velocity.y < 0) // 낙하 중일 때만 속도 제어
        {
            float newFallSpeed = rb.velocity.y - fallAcceleration * Time.fixedDeltaTime;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(newFallSpeed, -maxFallSpeed));
        }
    }
}
