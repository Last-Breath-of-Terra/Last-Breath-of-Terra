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

            TurnOnOutline();
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
        
        Volume volume = Camera.main.GetComponent<Volume>();
        if (volume.profile.TryGet(out bloom))
        {
            DOTween.To(() => bloom.scatter.value, 
                    x => bloom.scatter.value = x, 
                    1f, // 최종 목표값: 1.0
                    lifeInfuserData.infusionDuration); // Duration 설정
        }
        DOTween.To(
            () => mat.GetFloat("_Thickness"),
            x => mat.SetFloat("_Thickness", x),
            7f,
            lifeInfuserData.infusionDuration
        );

        DOVirtual.DelayedCall(lifeInfuserData.infusionDuration, CompleteInfusion);

    }

    private void CompleteInfusion()
    {
        // Infusion 완료 로직
        Color baseColor = mat.GetColor("_AllGlowColor");

        Volume volume = Camera.main.GetComponent<Volume>();
        Bloom bloom = null;

        if (volume.profile.TryGet(out bloom))
        {
            DOTween.To(() => bloom.scatter.value, 
                    x => bloom.scatter.value = x, 
                    0.6f,
                    1f);
        }

        DG.Tweening.Sequence seq = DOTween.Sequence()
        .Append(DOTween.To(() => mat.GetFloat("_Enabled"), x => mat.SetFloat("_Enabled", x), 0, 0.1f))
        .Append(DOTween.To(() => mat.GetFloat("_Clear"), x => mat.SetFloat("_Clear", x), 1, 0.1f))
        .Append(DOTween.To(() => 1f, val => mat.SetColor("_AllGlowColor", baseColor * val), 1.5f, 2f))
        .OnComplete(() =>
        {
            mat.SetFloat("_Clear", 0f);

            lifeInfuserData.CompleteInfusion(infuserNumber, gameObject);
            InfuserManager.Instance.canInfusion[infuserNumber] = false;
        });
        if (_playerController != null)
        {
            _playerController.SetCanMove(true);
        }
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

        TurnOffOutline();
    }

    // 외곽선 천천히 켜기 (Enable=1, Thickness=3)
    private void TurnOnOutline()
    {
        if (mat == null) return;

        enableTween?.Kill();
        thicknessTween?.Kill();

        enableTween = DOTween.To(() => mat.GetFloat("_Enabled"),
                                 x => mat.SetFloat("_Enabled", x),
                                 1f,
                                 0.5f);

        thicknessTween = DOTween.To(() => mat.GetFloat("_Thickness"),
                                    x => mat.SetFloat("_Thickness", x),
                                    1.5f,
                                    0.5f);
    }

    // 외곽선 천천히 끄기 (Enable=0, Thickness=0)
    private void TurnOffOutline()
    {
        if (mat == null) return;

        enableTween?.Kill();
        thicknessTween?.Kill();

        enableTween = DOTween.To(() => mat.GetFloat("_Enabled"),
                                 x => mat.SetFloat("_Enabled", x),
                                 0f,
                                 0.5f);

        thicknessTween = DOTween.To(() => mat.GetFloat("_Thickness"),
                                    x => mat.SetFloat("_Thickness", x),
                                    0f,
                                    0.5f);
    }
}
