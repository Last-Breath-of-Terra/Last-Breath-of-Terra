using UnityEngine;

public class ArcRenderer : MonoBehaviour
{
    public LineRenderer glowLineRenderer;  // Glow 효과 LineRenderer
    public LineRenderer brightLineRenderer;  // 밝은 중앙선 LineRenderer
    public ParticleSystem ps;  // ✨ 빛나는 끝점 효과
    public int segments = 80;  // 부드러운 곡선 세그먼트 수
    public float radius = 2.0f;  // 반원의 반지름
    public float drawDuration = 1.5f;  // 그려지는 속도

    private float elapsedTime = 0f;

    void Start()
    {
        if (glowLineRenderer == null || brightLineRenderer == null)
        {
            Debug.LogError("LineRenderer가 할당되지 않았습니다!");
            return;
        }

        glowLineRenderer.positionCount = 0;
        brightLineRenderer.positionCount = 0;

        if (ps != null)
        {
            ps.Stop();  // 초기에는 파티클 정지
        }
    }

    void Update()
    {
        if (elapsedTime < drawDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / drawDuration);
            DrawArc(progress);
        }
    }

    void DrawArc(float progress)
    {
        int visibleSegments = Mathf.FloorToInt(progress * segments);
        Vector3[] positions = new Vector3[visibleSegments];

        for (int i = 0; i < visibleSegments; i++)
        {
            float angle = Mathf.Lerp(Mathf.PI, 0, i / (float)(segments - 1));
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }

        // 🌟 두 개의 LineRenderer에 같은 위치 적용
        glowLineRenderer.positionCount = visibleSegments;
        glowLineRenderer.SetPositions(positions);

        brightLineRenderer.positionCount = visibleSegments;
        brightLineRenderer.SetPositions(positions);

        // ✨ 현재 "그려지는 끝점"에 파티클 시스템 이동
        if (ps != null && visibleSegments > 0)
        {
            Vector3 lastPosition = positions[visibleSegments - 1]; // 마지막 점의 위치
            ps.transform.position = lastPosition; // 파티클 이동
            if (!ps.isPlaying)
            {
                ps.Play();
            }
        }
    }
}
