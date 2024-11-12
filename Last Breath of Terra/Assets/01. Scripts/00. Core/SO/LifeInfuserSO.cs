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
    public Sprite infusionActiveUI;
    public Sprite infusionInactiveUI;
    public CinemachineVirtualCamera virtualCamera;

    [SerializeField] private int infusedLifeCount;

    void Awake()
    {
        DOTween.Init();
    }

    /*
     * 활성화 시작 시 호출
     */
    public void StartInfusion(Slider infusionSlider, int infuserNumber)
    {
        infusionSlider.gameObject.SetActive(true);
        currentTween = infusionSlider.DOValue(1, infusionDuration).OnComplete(() => CompleteInfusion(infusionSlider, infuserNumber));
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
    public virtual void CompleteInfusion(Slider infusionSlider, int infuserNumber)
    {
        Debug.Log("infusion completed");
        //state 복귀
        DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
        infusedLifeCount++;
        infusionSlider.gameObject.SetActive(false);
        infusionSlider.value = 0;
    }


    /*
     * 활성화 중지 시 호출
     */
    public void StopInfusion(Slider infusionSlider)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            infusionSlider.value = 0;
            DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);

            Debug.Log("infusion stopped");
        }
    }


}