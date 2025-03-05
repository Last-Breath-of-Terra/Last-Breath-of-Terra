using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;


[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/LifeInfuser")]
public class LifeInfuserSO : ScriptableObject
{
    public float infusionDuration;
    public float infusionWaitTime;
    public float defaultLensSize;
    public float targetLensSize;

    public Sprite[] InfuserActiveImage;

    public Sprite[] InfuserInactiveImage;

    //public CinemachineVirtualCamera virtualCamera;
    //public Canvas infuserActivationCanvas;
    //public GameObject InfuserStatusUI;
    //public Image infuserActivationUI;
    public Sprite activeIcon;
    public Sprite inactiveIcon;
    public Material defaultMaterial;
    public Material sacrificeMaterial;

    public int lineRendererSegments = 100;

    public int infusedLifeCount;
    private Tween currentTween;


    void Awake()
    {
        DOTween.Init();
    }

    /*
     * 활성화 시작 시 호출\
     */
    public virtual void StartInfusion(int infuserNumber, GameObject targetInfuser)
    {
        Debug.Log("start infusion");
        InfuserManager.Instance.infuserActivationCanvas.gameObject.transform.position =
            targetInfuser.transform.position;
        SetUIForInfuserStatus(true);
        InfuserManager.Instance.infuserActivationCanvas.gameObject.SetActive(true);

        //currentTween = DOTween.To(() => 0f, x => InfuserManager.Instance.infuserActivation.GetComponent<Image>().fillAmount = x, 1f, infusionDuration);
        AudioManager.instance.PanSoundLeftToRight("breath_action_being", infusionDuration);
        //infuserActivationUI.DOValue(1, infusionDuration).OnComplete(() => CompleteInfusion(infuserActivationUI, infuserNumber));

        float progress = 0f;
        
        InfuserManager.Instance.glowLineRenderer.positionCount = 0;
        InfuserManager.Instance.brightLineRenderer.positionCount = 0;
        
        currentTween = DOTween.To(() => progress, x => progress = x, 1f, infusionDuration)
            .OnStart(() => 
            {
                DrawArc(1f, targetInfuser.transform.position, InfuserManager.Instance.radius, InfuserManager.Instance.backLineRenderer);
            })
            .OnUpdate(() => 
            {
                DrawArc(progress, targetInfuser.transform.position, InfuserManager.Instance.radius, InfuserManager.Instance.brightLineRenderer, InfuserManager.Instance.gaugeParticle);
                DrawArc(progress, targetInfuser.transform.position, InfuserManager.Instance.radius, InfuserManager.Instance.glowLineRenderer);
            })
            .OnComplete(() =>
            {
                if (InfuserManager.Instance.gaugeParticle != null)
                {
                    InfuserManager.Instance.gaugeParticle.Stop(); // 완료 후 파티클 정지 가능
                }
            });
    }

    /*
     * 활성화 완료 시 호출
     */
    public virtual void CompleteInfusion(int infuserNumber, GameObject targetInfuser, int infuserType)
    {
        InfuserManager.Instance.objectParticle.transform.position = targetInfuser.transform.position;
        InfuserManager.Instance.objectParticle.Play();
        AudioManager.instance.PlayPlayer("breath_action_end", 0f);
        Debug.Log("play particle");
        
        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage[infuserType];
        targetInfuser.GetComponent<SpriteRenderer>().material = sacrificeMaterial;
        InfuserManager.Instance.infuserStatusChild[infuserNumber].GetComponent<Image>().color =
            new Color(1, 1, 1, 0.8f);

        Debug.Log("infusion completed");
        CinemachineVirtualCamera virtualCamera = InfuserManager.Instance.virtualCamera;
        //state 복귀
        DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
        infusedLifeCount++;
        InfuserManager.Instance.infuserActivationCanvas.gameObject.SetActive(false);
        InfuserManager.Instance.infuserActivation.GetComponent<Image>().fillAmount = 0.126f;
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
            AudioManager.instance.StopCancelable(audioSource);
            InfuserManager.Instance.infuserActivation.GetComponent<Image>().fillAmount = 0;
            DOTween.To(() => targetLensSize, x => InfuserManager.Instance.virtualCamera.m_Lens.OrthographicSize = x,
                defaultLensSize, 0.3f);
            SetUIForInfuserStatus(false);

            Debug.Log("infusion stopped");
        }
    }

    /*
     * 자식 오브젝트 투명도 설정
     */
    public void SetUIForInfuserStatus(bool isStart)
    {
        Debug.Log("setting UI for Infuser");
        float transparency;
        Vector3 canvasScale = InfuserManager.Instance.infuserStatus.transform.lossyScale;
        if (isStart)
        {
            transparency = 0.3f;
            canvasScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            transparency = -0.3f;
            canvasScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        DOTween.To(() => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale,
            x => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale = x, canvasScale, 0.1f);
        SetUITransparency(transparency);
    }

    public void SetUITransparency(float transparency)
    {
        foreach (Transform child in InfuserManager.Instance.infuserStatusChild)
        {
            Image image = child.GetComponent<Image>();
            if (image != null) // && !image.gameObject.CompareTag("Cursor"))
            {
                child.gameObject.GetComponent<Image>().color += new Color(1f, 1f, 1f, transparency);
            }
            // 자식의 자식들까지 재귀적으로 탐색
            //SetUITransparency(child, transparency);
        }
    }

    void DrawArc(float progress, Vector3 targetPosition, float radius, LineRenderer lineRenderer, [CanBeNull] ParticleSystem gaugeParticle = null)
    {
        int visibleSegments = Mathf.FloorToInt(progress * lineRendererSegments);
        Vector3[] positions = new Vector3[visibleSegments];

        for (int i = 0; i < visibleSegments; i++)
        {
            float angle = Mathf.Lerp(Mathf.PI, 0, i / (float)(lineRendererSegments - 1)); // 🔄 왼쪽 → 오른쪽 방향
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius - 6.5f, 0) + targetPosition; // 위치 이동
        }

        lineRenderer.positionCount = visibleSegments;
        lineRenderer.SetPositions(positions);

        if (gaugeParticle != null && visibleSegments > 0)
        {
            Vector3 lastPosition = positions[visibleSegments - 1];
            gaugeParticle.transform.position = lastPosition;
            if (gaugeParticle.GetComponent<ParticleSystem>().isPlaying)
            {
                gaugeParticle.Play();
            }
        }
    }

}