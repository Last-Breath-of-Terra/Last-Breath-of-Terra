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
    public GameObject newGameUI;
    public Image fadePanel;

    public TextMeshProUGUI pressAnyKeyText;
    public RectTransform titleTextRect;
    public Animator titleAnim;
    public Animator backgroundAnim;
    public ParticleSystem flameGaugeParticle;

    private enum TitleState { Intro, SaveSelect, ConfirmNewGame }
    private TitleState currentState = TitleState.Intro;

    private float holdTime = 0f;
    private const float holdThreshold = 2f;
    private bool isHoldingSpace = false;

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
            pressAnyKeyText.color = new Color(1, 1, 1, alpha);
        }
    }

    private IEnumerator TransitionToSaveSelect()
    {
        currentState = TitleState.SaveSelect;

        titleAnim.SetTrigger("ToSaveSelect");
        backgroundAnim.SetTrigger("ToSaveSelect");
        introUI.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        saveSelectUI.SetActive(true);
    }

    public void OnSaveSlotSelected()
    {
        currentState = TitleState.ConfirmNewGame;
        saveSelectUI.SetActive(false);
        newGameUI.SetActive(true);
    }

    private void HandleNewGameInput()
    {
        // 오른쪽 클릭 누르기 시작
        if (Input.GetMouseButtonDown(1))
        {
            isHoldingSpace = true;
            holdTime = 0f;
        }

        // 오른쪽 클릭 유지 중
        if (isHoldingSpace && Input.GetMouseButton(1))
        {
            holdTime += Time.deltaTime;
            float t = Mathf.Clamp01(holdTime / holdThreshold);
            UpdateFlameGauge(t);

            if (holdTime >= holdThreshold)
            {
                isHoldingSpace = false;
                StartCoroutine(LoadStoryScene()); // 게임 시작
            }
        }

        // 클릭 해제 시 아무 일도 하지 않음
        if (Input.GetMouseButtonUp(1))
        {
            isHoldingSpace = false;
            holdTime = 0f;
            UpdateFlameGauge(0f);
        }

        // A 키를 누르면 뒤로 감
        if (Input.GetKeyDown(KeyCode.A))
        {
            newGameUI.SetActive(false);
            saveSelectUI.SetActive(true);
            currentState = TitleState.SaveSelect;
        }
    }

    private void UpdateFlameGauge(float t)
    {
        if (flameGaugeParticle == null) return;

        var main = flameGaugeParticle.main;
        main.startSize = Mathf.Lerp(0.5f, 1.5f, t);
    }

    private IEnumerator LoadStoryScene()
    {
        fadePanel.gameObject.SetActive(true);

        fadePanel.color = new Color(1f, 1f, 1f, 0f);
        yield return fadePanel.DOFade(1f, 1f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return fadePanel.DOColor(Color.black, 1f).SetEase(Ease.InQuad).WaitForCompletion();

        // 선택된 슬롯에 대해 TitleStory 스토리 활성화
        StoryManager.Instance.ActivateStoryForScene("TitleStory");

        // 실제 게임 스토리 씬으로 전환
        SceneManager.LoadScene("StoryScene");
    }

    public void BackToIntroFromSave()
    {
        StopAllCoroutines();
        titleAnim.SetTrigger("ToIntro");
        backgroundAnim.SetTrigger("ToIntro");
        saveSelectUI.SetActive(false);
        introUI.SetActive(true);
        currentState = TitleState.Intro;
    }
}
