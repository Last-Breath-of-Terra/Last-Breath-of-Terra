using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using Cinemachine;
using DG.Tweening;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;




public class Stage2Clear : MonoBehaviour
{
    public Image fadeImage;
    public DataManager dataManager;
    public CinemachineVirtualCamera StageClearCamera;
    public float targetYOffset = 5f;
    public float duration = 2f;

    private void Start()
    {
        dataManager = FindObjectOfType<DataManager>();
        OnStageClear();
    }

    IEnumerator Fade(bool isCameraOn)
    {
        for (float i = 0; i < 1f; i += 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }
        StageClearCamera.gameObject.SetActive(isCameraOn);
        for (float i = 1; i > 0f; i -= 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }
    IEnumerator FadeAndThenMoveCamera()
    {
        // 페이드 먼저 실행
        yield return StartCoroutine(Fade(true)); // 코루틴 끝날 때까지 기다림

        // 트윈 시작
        var transposer = StageClearCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        Vector3 currentOffset = transposer.m_TrackedObjectOffset;

        DOTween.To(
            () => currentOffset.y,
            y => {
                currentOffset.y = y;
                transposer.m_TrackedObjectOffset = currentOffset;
            },
            targetYOffset,
            duration
        ).SetEase(Ease.OutCubic);
        
        yield return new WaitForSeconds(duration);

        StartCoroutine(Fade(false));
    }

    public void OnStageClear()
    {
        StartCoroutine(FadeAndThenMoveCamera());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int count = InfuserManager.Instance.activatedInfusers.Count(x => x);
        Debug.Log(count + " infusers activated");
        if (count >= 10) //추후 변경 필요
        {
            Debug.Log("stage cleared");
            DataManager.Instance.ModifyPlayerData(DataManager.Instance.playerIndex, 0, true);
            SceneManager.LoadScene("StageSelection");
        }
    }
}