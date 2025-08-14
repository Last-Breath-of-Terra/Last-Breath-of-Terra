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

    private static readonly int AlbedoColor = Shader.PropertyToID("_Color");

    private Coroutine fadeCoroutine;

    void Start()
    {
        SetAlphaImmediate(RightUIRenderer, 0f);
        SetAlphaImmediate(LeftUIRenderer, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (uiDirection == Direction.Right && RightUIRenderer != null)
            {
                StartFade(RightUIRenderer, 1f, 0.5f);
            }
            else if (uiDirection == Direction.Left && LeftUIRenderer != null)
            {
                StartFade(LeftUIRenderer, 1f, 0.5f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (RightUIRenderer != null) StartFade(RightUIRenderer, 0f, 0.5f);
            if (LeftUIRenderer != null) StartFade(LeftUIRenderer, 0f, 0.5f);
        }
    }

    // 즉시 알파 설정
    private void SetAlphaImmediate(Renderer targetRenderer, float alphaValue)
    {
        if (targetRenderer != null && targetRenderer.material.HasProperty(AlbedoColor))
        {
            Color color = targetRenderer.material.GetColor(AlbedoColor);
            color.a = alphaValue;
            targetRenderer.material.SetColor(AlbedoColor, color);
        }
    }

    // 서서히 알파 변화 시작
    private void StartFade(Renderer targetRenderer, float targetAlpha, float duration)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(targetRenderer, targetAlpha, duration));
    }

    // Lerp로 부드럽게 변화
    private IEnumerator FadeAlpha(Renderer targetRenderer, float targetAlpha, float duration)
    {
        if (targetRenderer == null || !targetRenderer.material.HasProperty(AlbedoColor)) yield break;

        Color startColor = targetRenderer.material.GetColor(AlbedoColor);
        float startAlpha = startColor.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            startColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            targetRenderer.material.SetColor(AlbedoColor, startColor);
            yield return null;
        }

        startColor.a = targetAlpha;
        targetRenderer.material.SetColor(AlbedoColor, startColor);
    }
}
