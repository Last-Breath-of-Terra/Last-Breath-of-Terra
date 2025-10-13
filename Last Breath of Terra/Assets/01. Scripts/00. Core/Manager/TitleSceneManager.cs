using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class TitleSceneManager : MonoBehaviour
{
    [Header("UI Container")]
    public GameObject introUI; // 타이틀 + Press Any Key
    public GameObject saveSelectUI; // 저장 슬롯 UI
    public GameObject newGameUI;

    [Header("UI Elements")]
    public GameObject newGameUI_New;
    public GameObject newGameUI_Remove;
    public Image fadePanel;
    public RectTransform confirmPanel;
    public Image pressAnyKeyImage;
    public RectTransform titleTextRect;
    public ParticleSystem flameGaugeParticle_New;
    public ParticleSystem flameGaugeParticle_Remove;

    [Header("Animation")]
    public Animator titleAnim;
    public Animator backgroundAnim;

    private enum TitleState { Intro, SaveSelect, ConfirmNewGame }
    private enum ConfirmIntent { Start, Delete }

    private TitleState currentState = TitleState.Intro;
    private ConfirmIntent currentIntent;

    private Tween panelTween;
    private Ease openEase = Ease.OutCubic;
    private float holdTime = 0f;
    private const float holdThreshold = 2f;
    private const float holdThresholdSpace = 0.5f;
    private bool isHoldingMouse = false;
    private bool isHoldingSpace = false;
    private int selectedSlotIndex = -1;

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

            case TitleState.SaveSelect:
                HandleSaveSelectInput();
                break;

            case TitleState.ConfirmNewGame:
                HandleNewGameInput();
                break;
        }
    }

    private void BlinkPressAnyKey()
    {
        if (pressAnyKeyImage != null)
        {
            float alpha = Mathf.PingPong(Time.time * 1.5f, 1f);
            Color imageColor = pressAnyKeyImage.color;
            imageColor.a = alpha;
            pressAnyKeyImage.color = imageColor;
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

    public void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        DataManager.Instance.playerIndex = index;
    }

    public void HandleSaveSelectInput()
    {
        // selectedSlotIndex가 아직 -1이면, EventSystem의 현재 선택에서 찾아서 즉시 셋업
        if (selectedSlotIndex < 0)
        {
            var cur = EventSystem.current?.currentSelectedGameObject;
            if (cur != null)
            {
                var btn = cur.GetComponent<SaveSlotButton>();
                if (btn != null) SelectSlot(btn.SlotIndex());
            }
        }

        // Space 누르기 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHoldingSpace = true;
            holdTime = 0f;
            UpdateFlameGauge(0f, flameGaugeParticle_Remove);
        }

        // Space 유지 중
        if (isHoldingSpace && Input.GetKey(KeyCode.Space))
        {
            holdTime += Time.deltaTime;
            float t = Mathf.Clamp01(holdTime / holdThresholdSpace);
            UpdateFlameGauge(t, flameGaugeParticle_Remove);
        }

        // Space 뗐을 때 분기 -> Confirm 진입
        if (isHoldingSpace && Input.GetKeyUp(KeyCode.Space))
        {
            isHoldingSpace = false;
            UpdateFlameGauge(0f, flameGaugeParticle_Remove);

            var intent = (holdTime >= holdThresholdSpace)
                ? ConfirmIntent.Delete
                : ConfirmIntent.Start;

            bool hasSave = DataManager.Instance.HasSave(selectedSlotIndex);

            if (intent == ConfirmIntent.Delete && !hasSave) return;

            StartCoroutine(EnterConfirmNewGame(intent));
        }
    }

    private IEnumerator EnterConfirmNewGame(ConfirmIntent intent)
    {
        currentState = TitleState.ConfirmNewGame;

        newGameUI.SetActive(true);

        confirmPanel.localScale = new Vector3(20f, 0f, 1f);

        yield return null; // 대기

        panelTween?.Kill();
        panelTween = confirmPanel
            .DOScaleY(7.5f, 0.5f)
            .SetEase(openEase);
        yield return panelTween.WaitForCompletion();

        currentIntent = intent;
        if (currentIntent == ConfirmIntent.Start)
            newGameUI_New.SetActive(true);
        else
            newGameUI_Remove.SetActive(true);

        isHoldingMouse = false;
        holdTime = 0f;
        UpdateFlameGauge(0f, flameGaugeParticle_New);
        UpdateFlameGauge(0f, flameGaugeParticle_Remove);
    }

    private IEnumerator ExitConfirmToSaveSelect()
    {
        panelTween?.Kill();
        yield return confirmPanel
            .DOScaleY(0f, 0.35f)
            .SetEase(Ease.InCubic)
            .WaitForCompletion();

        newGameUI_New.SetActive(false);
        newGameUI_Remove.SetActive(false);
        newGameUI.SetActive(false);

        currentState = TitleState.SaveSelect;
    }

    private void HandleNewGameInput()
    {
        // 어떤 파티클을 쓸지 의도에 따라 선택
        ParticleSystem gauge = (currentIntent == ConfirmIntent.Start)
            ? flameGaugeParticle_New
            : flameGaugeParticle_Remove;

        // 오른쪽 클릭 시작
        if (Input.GetMouseButtonDown(1))
        {
            isHoldingMouse = true;
            holdTime = 0f;
            UpdateFlameGauge(0f, gauge);
        }

        // 오른쪽 클릭 유지
        if (isHoldingMouse && Input.GetMouseButton(1))
        {
            holdTime += Time.deltaTime;
            float t = Mathf.Clamp01(holdTime / holdThreshold); // 2초 기준
            UpdateFlameGauge(t, gauge);

            if (holdTime >= holdThreshold)
            {
                isHoldingMouse = false;
                UpdateFlameGauge(0f, gauge);

                if (currentIntent == ConfirmIntent.Start)
                {
                    StartCoroutine(LoadGame());
                }
                else
                {
                    StartCoroutine(DeleteAndReturnToSave());
                }
            }
        }

        // 오른쪽 클릭 해제(취소)
        if (Input.GetMouseButtonUp(1))
        {
            isHoldingMouse = false;
            holdTime = 0f;
            UpdateFlameGauge(0f, gauge);
        }

        // 뒤로가기
        if (Input.GetKeyDown(KeyCode.A))
        {
            isHoldingMouse = false;
            holdTime = 0f;
            UpdateFlameGauge(0f, flameGaugeParticle_New);
            UpdateFlameGauge(0f, flameGaugeParticle_Remove);
            StartCoroutine(ExitConfirmToSaveSelect());
        }
    }

    private void UpdateFlameGauge(float t, ParticleSystem particle)
    {
        if (particle == null) return;

        var main = particle.main;
        main.startSize = Mathf.Lerp(1.7f, 5f, t);
    }

    private IEnumerator DeleteAndReturnToSave()
    {
        if (selectedSlotIndex >= 0)
            DataManager.Instance.RemovePlayerAtIndex(selectedSlotIndex);

        yield return StartCoroutine(ExitConfirmToSaveSelect());

        RefreshSaveSlotsUI();
    }

    private void RefreshSaveSlotsUI()
    {
        var buttons = saveSelectUI.GetComponentsInChildren<SaveSlotButton>(true);
        foreach (var b in buttons)
            b.Refresh();
    }

    private IEnumerator LoadGame()
    {
        fadePanel.gameObject.SetActive(true);

        fadePanel.color = new Color(1f, 1f, 1f, 0f);
        yield return fadePanel.DOFade(1f, 1f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return fadePanel.DOColor(Color.black, 1f).SetEase(Ease.InQuad).WaitForCompletion();

        int index = DataManager.Instance.playerIndex;

        bool hasProgress = DataManager.Instance.HasAnyStageCleared(index);

        if (hasProgress)
        {
            SceneManager.LoadScene("StageSelection");
        }
        else
        {
            StoryManager.Instance.ActivateStoryForScene("TitleStory");
            SceneManager.LoadScene("StoryScene");
        }
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
    
    public bool IsInSaveSelectState() => currentState == TitleState.SaveSelect;
}
