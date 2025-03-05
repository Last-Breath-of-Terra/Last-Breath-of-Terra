using UnityEngine;

public class ArcAnimation : MonoBehaviour
{
    public float speed = 2.0f; // 속도
    public float glowMin = 1.0f;
    public float glowMax = 3.0f;
    private Material mat;

    void Start()
    {
        mat = GetComponent<LineRenderer>().material;
    }

    void Update()
    {
        float glow = Mathf.PingPong(Time.time * speed, glowMax - glowMin) + glowMin;
        mat.SetFloat("_GlowIntensity", glow);
    }
}