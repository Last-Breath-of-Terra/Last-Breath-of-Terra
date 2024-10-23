using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;


[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/Life Infuser")]
public class LifeInfuserSO : ScriptableObject
{
    public float infusionDuration;
    public float infusionWaitTime;
    public float CooldownTimer;

    private Tween currentTween;
    [SerializeField]
    private int infusedLifeCount;
    void Awake()
    {
        DOTween.Init();
    }

    
    public void StartInfusion(Slider infusionSlider,ref bool canInfusion)
    {
        infusionSlider.gameObject.SetActive(true);
        canInfusion = false;
        currentTween = infusionSlider.DOValue(1, infusionDuration).OnComplete(() =>
        {
            infusedLifeCount++;
            infusionSlider.gameObject.SetActive(false);
            infusionSlider.value = 0;
            // Invoker.Invoke<>("StartInfusionCooldown", CooldownTimer);
        });

    }

    public void StopInfusion(Slider infusionSlider)
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            infusionSlider.value = 0;
        }
    }

    public void SpawnObstacle(GameObject[] obstacleSprites)
    {
        foreach (GameObject obstacle in obstacleSprites)
        {
            obstacle.SetActive(true); // 각 GameObject 활성화
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
