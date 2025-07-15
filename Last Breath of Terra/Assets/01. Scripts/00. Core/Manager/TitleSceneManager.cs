using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TitleSceneManager : MonoBehaviour
{
    public GameObject introUI; // 타이틀 + Press Any Key
    public GameObject saveSelectUI; // 저장 슬롯 UI
    public GameObject newGameUI; // 새 게임 시작하시겠습니까? UI

    public TextMeshProUGUI pressAnyKeyText;
    public RectTransform titleTextRect;
    public Animator transitionAnimator;

    private enum TitleState { Intro, SaveSelect, ConfirmNewGame }
    private TitleState currentState = TitleState.Intro;

    private void Start()
    {
        introUI.SetActive(true);
        saveSelectUI.SetActive(false);
        newGameUI.SetActive(false);
    }

    private void Update()
    {
        switch (currentState)
        {
            case TitleState.Intro:
                BlinkPressAnyKey();
                if (Input.anyKeyDown)
                {
                    StartCoroutine(TransitionToSaveSelect());
                }
                break;

            case TitleState.ConfirmNewGame:
                HandleNewGameInput();
                break;
        }
    }

    private void BlinkPressAnyKey()
    {
        if (pressAnyKeyText != null)
        {
            float alpha = Mathf.PingPong(Time.time * 1.5f, 1f);
            pressAnyKeyText.color = new Color(0, 0, 0, alpha);
        }
    }

    private IEnumerator TransitionToSaveSelect()
    {
        currentState = TitleState.SaveSelect;

        transitionAnimator.SetTrigger("ToSaveSelect");
        introUI.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        saveSelectUI.SetActive(true);
    }

    public void OnSaveSlotSelected()
    {
        StartCoroutine(TransitionToNewGameConfirm());
    }

    private IEnumerator TransitionToNewGameConfirm()
    {
        currentState = TitleState.ConfirmNewGame;
        yield return new WaitForSeconds(1f);
        saveSelectUI.SetActive(false);
        newGameUI.SetActive(true);
    }

    private float holdTime = 0f;
    private const float holdThreshold = 2f;

    private void HandleNewGameInput()
    {
        if (Input.GetMouseButton(1))
        {
            holdTime += Time.deltaTime;
            if (holdTime >= holdThreshold)
            {
                LoadStoryScene();
            }
        }
        else
        {
            holdTime = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 다시 저장 슬롯 화면으로 돌아감
            newGameUI.SetActive(false);
            saveSelectUI.SetActive(true);
            currentState = TitleState.SaveSelect;
        }
    }

    public void LoadStoryScene()
    {
        // 선택된 슬롯에 대해 TitleStory 스토리 활성화
        StoryManager.Instance.ActivateStoryForScene("TitleStory");

        // 실제 게임 스토리 씬으로 전환
        SceneManager.LoadScene("StoryScene");
    }

    public void BackToIntroFromSave()
    {
        StopAllCoroutines();
        transitionAnimator.SetTrigger("ToIntro");
        saveSelectUI.SetActive(false);
        introUI.SetActive(true);
        currentState = TitleState.Intro;
    }
}
