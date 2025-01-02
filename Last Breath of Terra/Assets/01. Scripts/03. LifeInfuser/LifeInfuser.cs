using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class LifeInfuser : MonoBehaviour
{
    public LifeInfuserSO lifeInfuserData;
    public GameObject[] obstacleSprites;
    public int infuserNumber;
    public int infuserType;
    //public bool[] canInfusion;
    private Tween startTween;

    private PlayerController _playerController;
    private Material mat;
    private Tween enableTween;
    private Tween thicknessTween;
    private Bloom bloom;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat("_Enabled", 0f);
        mat.SetFloat("_Thickness", 0f);
        Debug.Log(InfuserManager.Instance.gameObject.name);

        InfuserManager.Instance.canInfusion[infuserNumber] = true;
        InfuserManager.Instance.infuser[infuserNumber] = gameObject;
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player") && InfuserManager.Instance.canInfusion[infuserNumber])
        {
            _playerController = collision.GetComponent<PlayerController>();
            startTween = DOVirtual.DelayedCall(lifeInfuserData.infusionWaitTime, () =>
            {
                PrepareInfusion();
            });

            GameManager.Instance._shaderManager.TurnOnOutline(mat, 3f, 0.5f);
        }
    }
    private void PrepareInfusion()
    {
        Debug.Log("Prepare Infusion");

        if (_playerController != null)
        {
            AudioManager.instance.PlaySFX("breath_action_start", gameObject.GetComponent<AudioSource>(), gameObject.transform);
            _playerController.SetCanMove(false);
        }
        DOTween.To(() => lifeInfuserData.defaultLensSize, x => InfuserManager.Instance.virtualCamera.m_Lens.OrthographicSize = x, lifeInfuserData.targetLensSize, 0.5f);
        lifeInfuserData.StartInfusion(infuserNumber, gameObject);
        lifeInfuserData.SpawnObstacle(obstacleSprites);
        
        GameManager.Instance._shaderManager.PlayInfusionSequence(
            mat, 
            Camera.main.GetComponent<Volume>(), 
            lifeInfuserData.infusionDuration, 
            CompleteInfusion
        );
    }

    private void CompleteInfusion()
    {
        Debug.Log("완료되었습니당!");
        GameManager.Instance._shaderManager.CompleteInfusionEffect(
        mat,
        Camera.main.GetComponent<Volume>(),
        () =>
        {
            // Infusion 완료 후 처리
            lifeInfuserData.CompleteInfusion(infuserNumber, gameObject, infuserType);
            InfuserManager.Instance.canInfusion[infuserNumber] = false;

            if (_playerController != null)
            {
                _playerController.SetCanMove(true);
            }
        });
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (startTween != null)
        {
            startTween.Kill();
        }
        lifeInfuserData.StopInfusion(gameObject.GetComponent<AudioSource>());
        if (_playerController != null)
        {
            _playerController.SetCanMove(true);
        }

        GameManager.Instance._shaderManager.TurnOffOutline(mat, 0.5f);
    }
}
