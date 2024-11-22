using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;


[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/LifeInfuser")]
public class LifeInfuserSO : ScriptableObject
{
    public PlayerController playerController;
    public float infusionDuration;
    public float infusionWaitTime;
    public float defaultLensSize;
    public float targetLensSize;
    public Tween currentTween;
    public GameObject targetInfuser;
    public GameObject player;
    public CinemachineVirtualCamera virtualCamera;
    public Canvas infuserActivationCanvas;
    public GameObject InfuserStatusUI;
    public Image infuserActivationUI;
    
    public int infusedLifeCount;

    void Awake()
    {
        DOTween.Init();
    }

    /*
     * 활성화 시작 시 호출
     */
    public virtual void StartInfusion(int infuserNumber)
    {
        SetUIForInfuserStatus(true);
        infuserActivationCanvas.gameObject.SetActive(true);
        
        currentTween = DOTween.To(() => 0.126f, x => infuserActivationUI.GetComponent<Image>().fillAmount = x, 0.875f, infusionDuration).OnComplete(() => CompleteInfusion(infuserNumber));

        //infuserActivationUI.DOValue(1, infusionDuration).OnComplete(() => CompleteInfusion(infuserActivationUI, infuserNumber));
    }
    public void SpawnObstacle(GameObject[] obstacleSprites)
    {
        foreach (GameObject obstacle in obstacleSprites)
        {
            obstacle.SetActive(true); // 각 GameObject 활성화
            Obstacle obstacleComponent = obstacle.GetComponent<Obstacle>();
            if (obstacleComponent != null)
            {
                obstacleComponent.ReactivateObstacle(obstacle.transform.position);
            }
        }
    }

    /*
     * 활성화 완료 시 호출
     */
    public virtual void CompleteInfusion(int infuserNumber)
    {
        Debug.Log("infusion completed");
        
        //state 복귀
        DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
        infusedLifeCount++;
        infuserActivationUI.gameObject.SetActive(false);
        infuserActivationUI.GetComponent<Image>().fillAmount = 0;
        SetUIForInfuserStatus(false);
    }


    /*
     * 활성화 중지 시 호출
     */
    public void StopInfusion()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            AudioManager.instance.StopCancelable(player.GetComponent<AudioSource>());
            infuserActivationUI.GetComponent<Image>().fillAmount = 0;
            DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
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
        Transform transform = InfuserStatusUI.transform;
        Vector3 canvasScale = transform.lossyScale;
        if (isStart)
        {
            transparency = 0.3f;
            canvasScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        else
        {
            transparency = -0.3f;
            canvasScale = new Vector3(1f, 1f, 1f);

        }
        DOTween.To(() => InfuserStatusUI.GetComponent<RectTransform>().localScale, x => InfuserStatusUI.GetComponent<RectTransform>().localScale = x, canvasScale, 0.1f);
        //InfuserStatusUI.GetComponent<RectTransform>().localScale = canvasScale;
        SetUITransparency(transform, transparency);

    }
    public void SetUITransparency(Transform parent, float transparency)
    {
        // 부모가 null이 아니면 진행
        if (parent == null)
            return;

        // 자식 오브젝트들을 모두 탐색
        foreach (Transform child in parent)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                child.gameObject.GetComponent<Image>().color += new Color(1f, 1f, 1f, transparency);
            }
            // 자식의 자식들까지 재귀적으로 탐색
            SetUITransparency(child, transparency);
        }
    }


}