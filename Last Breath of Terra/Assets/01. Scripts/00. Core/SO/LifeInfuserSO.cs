using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/LifeInfuser")]
public class LifeInfuserSO : ScriptableObject
{
    public float infusionDuration;
    public float infusionWaitTime;
    public float defaultLensSize;
    public float targetLensSize;
    public float arcHeight;
    public float arcOffsetX;

    public Sprite[] InfuserActiveImage;
    public Sprite[] InfuserInactiveImage;

    public Sprite activeIcon;
    public Sprite inactiveIcon;
    public Material defaultMaterial;
    public Material sacrificeMaterial;

    public int lineRendererSegments = 100;
    public int infusedLifeCount;

    private Tween currentTween;

    // UI tween control
    private bool isUIExpanded = false;
    private bool isStartInfusion = false;
    public float uiTweenDuration = 0.5f;


    void Awake()
    {
        DOTween.Init();
    }

    /*
     * 활성화 시작 시 호출
     */
    public virtual void StartInfusion(int infuserNumber, GameObject targetInfuser)
    {
        isStartInfusion = true;
        UnityEngine.Debug.Log("start infusion");
        SetUIForInfuserStatus(true);
        InfuserManager.Instance.ArcEffect.gameObject.SetActive(true);
        InfuserManager.Instance.gaugeParticle.Play();

        AudioManager.Instance.PanSoundLeftToRight("breath_action_being", infusionDuration);
        float progress = 0f;
        InfuserManager.Instance.glowLineRenderer.positionCount = 0;
        InfuserManager.Instance.brightLineRenderer.positionCount = 0;


        currentTween = DOTween.To(() => progress, x => progress = x, 1f, infusionDuration)
            .SetEase(Ease.Linear)
            .OnStart(() =>
            {
                DrawArc(1f, targetInfuser.transform.position, InfuserManager.Instance.radius,
                    InfuserManager.Instance.backLineRenderer);
            })
            .OnUpdate(() =>
            {
                float circularProgress = (1 - Mathf.Cos(progress * Mathf.PI)) / 2;
                DrawArc(circularProgress, targetInfuser.transform.position, InfuserManager.Instance.radius,
                    InfuserManager.Instance.brightLineRenderer, InfuserManager.Instance.gaugeParticle);
                DrawArc(circularProgress, targetInfuser.transform.position, InfuserManager.Instance.radius,
                    InfuserManager.Instance.glowLineRenderer);
            })
            .OnComplete(() =>
            {
                if (InfuserManager.Instance.gaugeParticle != null)
                {
                    InfuserManager.Instance.gaugeParticle.Stop();
                }
            });
    }

    /*
     * 활성화 완료 시 호출
     */
    public virtual void CompleteInfusion(int infuserNumber, GameObject targetInfuser, int infuserType)
    {
        InfuserManager.Instance.successParticle.transform.position = targetInfuser.transform.position;
        InfuserManager.Instance.successParticle.Play();
        AudioManager.Instance.PlayPlayer("breath_action_end", 0f);
        UnityEngine.Debug.Log("play particle");

        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage[infuserType];
        targetInfuser.GetComponent<SpriteRenderer>().material = sacrificeMaterial;
        InfuserManager.Instance.infuserStatusChild[infuserNumber].GetComponent<UnityEngine.UI.Image>().color =
            new Color(1, 1, 1, 0.8f);

        UnityEngine.Debug.Log("infusion completed");
        isStartInfusion = false;
        CinemachineVirtualCamera virtualCamera = InfuserManager.Instance.virtualCamera;
        DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
        infusedLifeCount++;
        InfuserManager.Instance.ArcEffect.gameObject.SetActive(false);
        InfuserManager.Instance.activatedInfusers[infuserNumber] = true;

        SetUIForInfuserStatus(false);
    }

    /*
     * 활성화 중지 시 호출
     */
    public void StopInfusion(AudioSource audioSource)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            AudioManager.Instance.StopCancelable(audioSource);
            DOTween.To(() => targetLensSize, x => InfuserManager.Instance.virtualCamera.m_Lens.OrthographicSize = x,
                defaultLensSize, 0.3f);
            SetUIForInfuserStatus(false);

            UnityEngine.Debug.Log("infusion stopped");
            isStartInfusion = false;
        }
    }

    /*
     * 활성화 완료 여부 체크
     */
    public bool CheckActiveInfusionCount()
    {
        foreach (var activatedInfuser in InfuserManager.Instance.activatedInfusers)
        {
            if (activatedInfuser == false)
                return false;
        }

        return true;
    }

    /*
     * 자식 오브젝트 투명도 설정 및 스케일 처리
     */
    public void SetUIForInfuserStatus(bool isStart)
    {
        isUIExpanded = isStart;

        float transparency = isStart ? 0.3f : -0.3f;
        Vector3 canvasScale = isUIExpanded ? new Vector3(1f, 1f, 1f) : new Vector3(0.5f, 0.5f, 0.5f);

        if (!isUIExpanded && isStartInfusion) return;

        DOTween.To(
            () => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale,
            x => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale = x,
            canvasScale,
            uiTweenDuration
        ).SetEase(Ease.InOutCubic);

        SetUITransparency(transparency);
    }

    public void SetUITransparency(float transparency)
    {
        foreach (Transform child in InfuserManager.Instance.infuserStatusChild)
        {
            var image = child.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color += new Color(1f, 1f, 1f, transparency);
            }
        }
    }

    void DrawArc(float progress, Vector3 targetPosition, float radius, LineRenderer lineRenderer,
        [CanBeNull] ParticleSystem gaugeParticle = null)
    {
        targetPosition = new Vector3(targetPosition.x + arcOffsetX, targetPosition.y, targetPosition.z);
        int visibleSegments = Mathf.FloorToInt(progress * lineRendererSegments);
        Vector3[] positions = new Vector3[visibleSegments];

        for (int i = 0; i < visibleSegments; i++)
        {
            float angle = Mathf.Lerp(Mathf.PI, 0, i / (float)(lineRendererSegments - 1));
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius - arcHeight, 0) +
                           targetPosition;
        }

        lineRenderer.positionCount = visibleSegments;
        lineRenderer.SetPositions(positions);

        if (gaugeParticle != null && visibleSegments > 0)
        {
            Vector3 lastPosition = positions[visibleSegments - 1];
            gaugeParticle.transform.position = lastPosition;
        }
    }
}