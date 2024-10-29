using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;


[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/Life Infuser")]
public class LifeInfuserSO : ScriptableObject
{
    public PlayerController playerController;
    public float infusionDuration;
    public float infusionWaitTime;
    public float CooldownTimer;
    public bool canInfusion;
    public float defaultLensSize;
    public float targetLensSize;
    public Tween currentTween;
    public CinemachineVirtualCamera virtualCamera;
    
    [SerializeField]
    private int infusedLifeCount;
    void Awake()
    {
        DOTween.Init();
    }

    
    public void StartInfusion(Slider infusionSlider)
    {
        infusionSlider.gameObject.SetActive(true);
        currentTween = infusionSlider.DOValue(1, infusionDuration).OnComplete(() => CompleteInfusion(infusionSlider));
    }

    private void CompleteInfusion(Slider infusionSlider)
    {
        Debug.Log("infusion completed");
        DOTween.To(() => targetLensSize, x => virtualCamera.m_Lens.OrthographicSize = x, defaultLensSize, 0.3f);
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }
        infusedLifeCount++;
        infusionSlider.gameObject.SetActive(false);
        infusionSlider.value = 0;
        //canInfusion = false; // 상태 업데이트
    }


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

    public void SpawnObstacle(GameObject[] obstacleSprites)
    {
        foreach (GameObject obstacle in obstacleSprites)
        {
            obstacle.SetActive(true); // 각 GameObject 활성화
            Enemy enemyComponent = obstacle.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.ReactivateEnemy(obstacle.transform.position);
            }
        }
    }

    /*
     * canInfusion이 활성화 되는 부분에 대해 좀 더 구체적인 계획이 필요할 것 같아요.
     */
    private void InfusionCooldown(ref bool canInfusion)
    {
        canInfusion = true;
    }
}
