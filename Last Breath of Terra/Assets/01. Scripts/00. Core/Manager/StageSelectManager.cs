using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class StageSelectManager : MonoBehaviour
{
    public Animator etherisAnim;

    [Header("UI Elements")]
    public Image backgroundImage;
    public Image etherisImage;
    public TextMeshProUGUI stageNameText;

    [Header("Stage Data")]
    public StageSelectInfo[] stages;

    [Header("Fade Setting")]
    public RectTransform fadeOverlay;
    public float fadeDuration = 1f;

    [Header("FX Elements")]
    public ParticleSystem flameGaugeParticle;
    public Image fadePanel;
    public Image whiteFadePanel;

    private int currentIndex = 0;

    private float holdTime = 0f;
    private float screenWidth = 1920f;
    private const float holdThreshold = 2f;
    private bool isHoldingSpace = false;

    void Start()
    {

        UpdateStageDisplay();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // 스테이지 좌/우 이동
        if (Input.GetKeyDown(KeyCode.D) && currentIndex < stages.Length - 1)
        {
            currentIndex++;
            StartCoroutine(TransitionToStage(1));  // 오른쪽으로 이동
        }
        else if (Input.GetKeyDown(KeyCode.A) && currentIndex > 0)
        {
            currentIndex--;
            StartCoroutine(TransitionToStage(-1)); // 왼쪽으로 이동
        }

        // 오른쪽 마우스 길게 누르기 처리
        if (Input.GetMouseButtonDown(1))
        {

            isHoldingSpace = true;
            holdTime = 0f;
        }

        if (isHoldingSpace && Input.GetMouseButton(1))
        {
            holdTime += Time.deltaTime;
            float t = Mathf.Clamp01(holdTime / holdThreshold);
            UpdateFlameGauge(t);
            UpdateWhiteFade(t);

            if (holdTime >= holdThreshold)
            {
                isHoldingSpace = false;
                etherisAnim.SetTrigger("Jump");
                ResetWhiteFade();
                StartCoroutine(LoadSelectedStage());
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            isHoldingSpace = false;
            holdTime = 0f;
            UpdateFlameGauge(0f);
            ResetWhiteFade();
        }
    }

    IEnumerator TransitionToStage(int direction)
    {
        float width = screenWidth + 800;

        // 방향에 따라 시작과 끝 위치 설정
        float startX = direction < 0 ? -width : width;
        float midX = 0f;
        float endX = -startX;

        // 시작 위치로 설정
        fadeOverlay.anchoredPosition = new Vector2(startX, 0);

        // DOTween 시퀀스 생성
        Sequence seq = DOTween.Sequence();

        // 1. 들어오기
        seq.Append(fadeOverlay.DOAnchorPos(new Vector2(midX, 0), fadeDuration / 2f).SetEase(Ease.InOutSine));

        // 2. 중앙 도달 시 스테이지 교체
        seq.AppendCallback(() =>
        {
            UpdateStageDisplay();
        });

        // 3. 나가기
        seq.Append(fadeOverlay.DOAnchorPos(new Vector2(endX, 0), fadeDuration / 2f).SetEase(Ease.InOutSine));

        // 4. 애니메이션 완료 후 위치 정리
        seq.OnComplete(() =>
        {
            fadeOverlay.anchoredPosition = new Vector2(endX, 0);
        });

        yield return seq.WaitForCompletion();
    }

    void UpdateStageDisplay()
    {
        var stage = stages[currentIndex];

        if (backgroundImage != null)
            backgroundImage.sprite = stage.background;

        if (stageNameText != null)
            stageNameText.text = stage.displayName;

        if (etherisImage != null)
            etherisImage.sprite = stage.etherisSprite;
    }

    private void UpdateFlameGauge(float t)
    {
        if (flameGaugeParticle == null) return;

        var main = flameGaugeParticle.main;
        main.startSize = Mathf.Lerp(1.5f, 5f, t);
    }

    private void UpdateWhiteFade(float t)
    {
        if (whiteFadePanel == null) return;

        Color color = whiteFadePanel.color;
        color.a = Mathf.Lerp(0f, 30f / 255f, t);
        whiteFadePanel.color = color;
    }

    private void ResetWhiteFade()
    {
        if (whiteFadePanel == null) return;

        Color color = whiteFadePanel.color;
        color.a = 0f;
        whiteFadePanel.color = color;
    }

    IEnumerator LoadSelectedStage()
    {
        yield return new WaitForSeconds(1f);

        fadePanel.gameObject.SetActive(true);

        fadePanel.color = new Color(1f, 1f, 1f, 0f);
        yield return fadePanel.DOFade(1f, 1f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return fadePanel.DOColor(Color.black, 1f).SetEase(Ease.InQuad).WaitForCompletion();

        StoryManager.Instance.ActivateStoryForScene(stages[currentIndex].sceneName + "Story");
        SceneManager.LoadScene("StoryScene");

        // SceneManager.LoadScene(stages[currentIndex].sceneName);
        yield return null;
    }
}