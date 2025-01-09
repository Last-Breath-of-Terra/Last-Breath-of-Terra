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
        InfuserManager.Instance.infuserActivationCanvas.gameObject.transform.position = targetInfuser.transform.position;
        SetUIForInfuserStatus(true);
        InfuserManager.Instance.infuserActivationCanvas.gameObject.SetActive(true);
        
        currentTween = DOTween.To(() => 0.126f, x => InfuserManager.Instance.infuserActivation.GetComponent<Image>().fillAmount = x, 0.875f, infusionDuration);
        AudioManager.instance.PanSoundLeftToRight("breath_action_being", infusionDuration);
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
    public virtual void CompleteInfusion(int infuserNumber, GameObject targetInfuser, int infuserType)
    {
        AudioManager.instance.PlayPlayer("breath_action_end", 0f);
        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage[infuserType];
        targetInfuser.GetComponent<SpriteRenderer>().material = sacrificeMaterial;
        InfuserManager.Instance.infuserStatusChild[infuserNumber].GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);

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
            DOTween.To(() => targetLensSize, x => InfuserManager.Instance. virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
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
        DOTween.To(() => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale, x => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale = x, canvasScale, 0.1f);
        SetUITransparency(transparency);

    }
    public void SetUITransparency(float transparency)
    {
        foreach (Transform child in InfuserManager.Instance.infuserStatusChild)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)// && !image.gameObject.CompareTag("Cursor"))
            {
                child.gameObject.GetComponent<Image>().color += new Color(1f, 1f, 1f, transparency);
            }
            // 자식의 자식들까지 재귀적으로 탐색
            //SetUITransparency(child, transparency);
        }
    }


}