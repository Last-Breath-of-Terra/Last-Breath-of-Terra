using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LifeInfuser : MonoBehaviour
{
    public GameObject camera;

    public Slider infusionSlider;
    public LifeInfuserSO lifeInfuserData;
    public GameObject[] obstacleSprites;
    
    private Tween startTween;

    
    [SerializeField]
    private bool canInfusion;

    private void Start()
    {
        lifeInfuserData.canInfusion = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player") && lifeInfuserData.canInfusion)
        {
            lifeInfuserData.playerController = collision.GetComponent<PlayerController>();
            //Invoke("PrepareInfusion", lifeInfuserData.infusionWaitTime);
            startTween = DOVirtual.DelayedCall(lifeInfuserData.infusionWaitTime, () =>
            {
                PrepareInfusion();
            });
        }
    }
    private void PrepareInfusion()
    {
        Debug.Log("Prepare Infusion");
        if (lifeInfuserData.playerController != null)
        {
            lifeInfuserData.playerController.SetCanMove(false);
        }
        lifeInfuserData.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        DOTween.To(() => lifeInfuserData.defaultLensSize, x => lifeInfuserData.virtualCamera.m_Lens.OrthographicSize = x, lifeInfuserData.targetLensSize, 0.5f);
        lifeInfuserData.StartInfusion(infusionSlider);
        lifeInfuserData.SpawnObstacle(obstacleSprites);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (startTween != null)
        {
            startTween.Kill();
        }
        lifeInfuserData.StopInfusion(infusionSlider);

    }
}
