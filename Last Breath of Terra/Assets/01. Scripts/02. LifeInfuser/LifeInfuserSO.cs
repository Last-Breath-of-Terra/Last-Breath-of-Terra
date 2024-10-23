using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


[CreateAssetMenu(fileName = "LifeInfuser", menuName = "ScriptableObject/Life Infuser")]
public class LifeInfuserSO : ScriptableObject
{
    public float infusionDuration;

    private Tween currentTween;
    [SerializeField]
    private int infusedLifeCount;
    void Awake()
    {
        DOTween.Init();
    }

    public void StartInfusion(Slider infusionSlider)
    {
        //infusionSlider.gameObject.SetActive(true);
        currentTween = infusionSlider.DOValue(1, infusionDuration).OnComplete(() =>
        {
            infusedLifeCount++;
         //   infusionSlider.gameObject.SetActive(false);
            infusionSlider.value = 0;
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
}
