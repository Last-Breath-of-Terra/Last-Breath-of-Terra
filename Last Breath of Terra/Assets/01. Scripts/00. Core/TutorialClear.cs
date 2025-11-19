using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;
using Cinemachine;
using DG.Tweening;
using System.Linq;


public class TutorialClear : MonoBehaviour
{
    public GameObject door;
    public Image fadeImage;
    public CinemachineVirtualCamera StageClearCamera;
    public float duration = 2f;

    private void Start()
    {
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

        // 투명해지기 시작
        SpriteRenderer spriteRenderer = door.GetComponent<SpriteRenderer>();
        // 2초 동안 알파값을 0으로 트윈 → 점점 투명해짐
        spriteRenderer.DOFade(0f, duration).OnComplete(() => door.SetActive(false));

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
        if (count >= 4) //추후 변경 필요
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().SetCanMove(false);
            StartCoroutine(ClearTutorical());
        }
    }

    IEnumerator ClearTutorical()
    {
        for (float i = 0; i < 1f; i += 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        DataManager.Instance.ModifyPlayerData(DataManager.Instance.playerIndex, 0, true);
        SceneManager.LoadScene("StageSelection");
    }
}