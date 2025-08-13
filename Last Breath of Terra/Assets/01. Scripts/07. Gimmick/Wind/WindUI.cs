using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindUI : MonoBehaviour
{
    public enum Direction
    {
        Right,
        Left
    }

    [Header("UI 오브젝트의 Particle System Renderer")]
    public Renderer RightUIRenderer;
    public Renderer LeftUIRenderer;

    [Header("UI 표시 방향")]
    public Direction uiDirection;

    // Particles/Standard Unlit 셰이더의 Albedo 컬러 속성명
    private static readonly int AlbedoColor = Shader.PropertyToID("_Color");

    void Start()
    {
        SetAlpha(RightUIRenderer, 0f);
        SetAlpha(LeftUIRenderer, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiDirection == Direction.Right && RightUIRenderer != null)
            {
                SetAlpha(RightUIRenderer, 1f);
            }
            else if (uiDirection == Direction.Left && LeftUIRenderer != null)
            {
                SetAlpha(LeftUIRenderer, 1f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetAlpha(RightUIRenderer, 0f);
            SetAlpha(LeftUIRenderer, 0f);
        }
    }

    private void SetAlpha(Renderer targetRenderer, float alphaValue)
    {
        if (targetRenderer != null && targetRenderer.material.HasProperty(AlbedoColor))
        {
            Color color = targetRenderer.material.GetColor(AlbedoColor);
            color.a = alphaValue;
            targetRenderer.material.SetColor(AlbedoColor, color);
        }
    }
}
