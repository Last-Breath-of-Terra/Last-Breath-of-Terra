using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;


public class LifeInfuser : MonoBehaviour
{
    //public GameObject camera;

    //public Image infuserActivationUI;
    //[FormerlySerializedAs("infusionSlider")] public Image infuserActivationUI;
    public StageLifeInfuserSO lifeInfuserData;
    public GameObject[] obstacleSprites;
    public int infuserNumber;
    
    private Tween startTween;
    
    private void Start()
    {
        //나중에 삭제 필요
        lifeInfuserData.canInfusion[infuserNumber] = true;
        
        lifeInfuserData.infuser[infuserNumber] = gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        //AudioManager.instance.PlaySFX("sfx_keyboardcorrect", gameObject.GetComponent<AudioSource>());
        if (collision.transform.CompareTag("Player") && lifeInfuserData.canInfusion[infuserNumber])
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;

            lifeInfuserData.targetInfuser = gameObject;
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
            AudioManager.instance.PlaySFX("breath_action_start", gameObject.GetComponent<AudioSource>(), gameObject.transform);
            lifeInfuserData.playerController.SetCanMove(false);
        }
        //lifeInfuserData.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        DOTween.To(() => lifeInfuserData.defaultLensSize, x => lifeInfuserData.virtualCamera.m_Lens.OrthographicSize = x, lifeInfuserData.targetLensSize, 0.5f);
        lifeInfuserData.StartInfusion(infuserNumber);
        lifeInfuserData.SpawnObstacle(obstacleSprites);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (startTween != null)
        {
            startTween.Kill();
        }
        lifeInfuserData.StopInfusion();
    }
}
