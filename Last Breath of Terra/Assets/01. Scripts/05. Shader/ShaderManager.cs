using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ShaderManager : MonoBehaviour
{
    // 외곽선 활성화
    public void TurnOnOutline(Material mat, float thickness = 3f, float duration = 0.5f)
    {
        if (mat == null) return;

        DOTween.To(() => mat.GetFloat("_Enabled"), 
                   x => mat.SetFloat("_Enabled", x), 
                   1f, 
                   duration);

        DOTween.To(() => mat.GetFloat("_Thickness"), 
                   x => mat.SetFloat("_Thickness", x), 
                   thickness, 
                   duration);
    }

    // 외곽선 비활성화
    public void TurnOffOutline(Material mat, float duration = 0.5f)
    {
        if (mat == null) return;

        DOTween.To(() => mat.GetFloat("_Enabled"), 
                   x => mat.SetFloat("_Enabled", x), 
                   0f, 
                   duration);

        DOTween.To(() => mat.GetFloat("_Thickness"), 
                   x => mat.SetFloat("_Thickness", x), 
                   0f, 
                   duration);
    }

    // Bloom 효과 점진적 적용
    public void AdjustBloom(Volume volume, float targetValue, float duration)
    {
        if (volume == null || !volume.profile.TryGet(out Bloom bloom)) return;

        DOTween.To(() => bloom.scatter.value, 
                   x => bloom.scatter.value = x, 
                   targetValue, 
                   duration);
    }

    // Shader 속성 점진적 변경 (Float)
    public void AnimateFloatProperty(Material mat, string propertyName, float targetValue, float duration)
    {
        if (mat == null) return;

        DOTween.To(() => mat.GetFloat(propertyName), 
                   x => mat.SetFloat(propertyName, x), 
                   targetValue, 
                   duration);
    }

    // Shader 속성 점진적 변경 (Color)
    public void AnimateColorProperty(Material mat, string propertyName, Color targetColor, float duration)
    {
        if (mat == null) return;

        DOTween.To(() => mat.GetColor(propertyName), 
                   x => mat.SetColor(propertyName, x), 
                   targetColor, 
                   duration);
    }

    public void LifeSacrificeEffect (Material mat, Volume volume, float infusionDuration)
    {
        if (mat == null || volume == null || !volume.profile.TryGet(out Bloom bloom)) return;

        Color baseColor = mat.GetColor("_AllGlowColor");

        Sequence sequence = DOTween.Sequence();

        // Bloom 효과 추가
        sequence.Append(DOTween.To(() => bloom.scatter.value, 
                                x => bloom.scatter.value = x, 
                                1f, 
                                infusionDuration));

        // Thickness 변경 추가
        sequence.Join(DOTween.To(() => mat.GetFloat("_Thickness"),
                                x => mat.SetFloat("_Thickness", x),
                                7f,
                                infusionDuration));

        // Material 속성 애니메이션 추가
        sequence.Append(DOTween.To(() => mat.GetFloat("_Clear"), 
                                x => mat.SetFloat("_Clear", x), 
                                1f, 
                                0.1f))
                .Append(DOTween.To(() => 1f, 
                                val => mat.SetColor("_AllGlowColor", baseColor * val), 
                                1.2f, 
                                1.5f))
                .OnComplete(() =>
                {
                    mat.SetFloat("_Clear", 0f);
                });
    }


    #region Infusion Methods
    // Infusion 효과
    public void PlayInfusionSequence(Material mat, Volume volume, float infusionDuration, System.Action onComplete)
    {
        if (mat == null || volume == null || !volume.profile.TryGet(out Bloom bloom)) return;

        Color baseColor = mat.GetColor("_AllGlowColor");

        Sequence sequence = DOTween.Sequence();

        // Bloom 효과 추가
        sequence.Append(DOTween.To(() => bloom.scatter.value, 
                                x => bloom.scatter.value = x, 
                                1f, 
                                infusionDuration));

        // Thickness 변경 추가
        sequence.Join(DOTween.To(() => mat.GetFloat("_Thickness"),
                                x => mat.SetFloat("_Thickness", x),
                                7f,
                                infusionDuration));

        // Material 속성 애니메이션 추가
        sequence.Append(DOTween.To(() => mat.GetFloat("_Clear"), 
                                x => mat.SetFloat("_Clear", x), 
                                1f, 
                                0.1f))
                .Append(DOTween.To(() => 1f, 
                                val => mat.SetColor("_AllGlowColor", baseColor * val), 
                                1.2f, 
                                1.5f))
                .OnComplete(() =>
                {
                    mat.SetFloat("_Clear", 0f);
                    onComplete?.Invoke();
                });
    }

    public void CompleteInfusionEffect(Material mat, Volume volume, System.Action onComplete)
    {
        if (mat == null || volume == null) return;

        Color baseColor = mat.GetColor("_AllGlowColor");

        // Bloom 효과 조정
        if (volume.profile.TryGet(out Bloom bloom))
        {
            DOTween.To(() => bloom.scatter.value, 
                    x => bloom.scatter.value = x, 
                    0.6f, 
                    1f);
        }

        // Material 속성 점진적 변경
        DOTween.Sequence()
            .Append(DOTween.To(() => mat.GetFloat("_Enabled"), x => mat.SetFloat("_Enabled", x), 0, 0.1f))
            .Append(DOTween.To(() => mat.GetFloat("_Clear"), x => mat.SetFloat("_Clear", x), 1, 0.1f))
            .Append(DOTween.To(() => 1f, val => mat.SetColor("_AllGlowColor", baseColor * val), 1.5f, 2f))
            .OnComplete(() =>
            {
                mat.SetFloat("_Clear", 0f);
                onComplete?.Invoke();
            });
    }
    #endregion
}
