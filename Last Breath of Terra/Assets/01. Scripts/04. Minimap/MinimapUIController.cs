using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class MiniMapUIController : MonoBehaviour
{
    public Image fadePanelA;
    public RectTransform minimapUI;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float dimAlpha = 0.7f;   // 어둡게 되는 정도
    public float fadeDuration = 0.4f;               // 어둡게/밝게 페이드 시간
    public float minimapFadeDuration = 0.3f;        // 미니맵 등장/퇴장 페이드 시간

    private CanvasGroup minimapGroup;
    private Sequence currentSeq;

    private void Awake()
    {
        minimapGroup = minimapUI.GetComponent<CanvasGroup>();
        if (minimapGroup == null) minimapGroup = minimapUI.gameObject.AddComponent<CanvasGroup>();

        // 초기 상태 정리
        ResetFadePanels();
    }

    public void ShowMinimap()
    {
        if (currentSeq != null && currentSeq.IsActive()) currentSeq.Kill();

        // 초기값 세팅
        fadePanelA.gameObject.SetActive(true);
        fadePanelA.color = new Color(0f, 0f, 0f, 0f);

        minimapUI.gameObject.SetActive(true);
        minimapGroup.alpha = 0f; // 투명한 상태에서 시작

        // 시퀀스: 화면 어둡게 → 미니맵 페이드인
        currentSeq = DOTween.Sequence()
            .Append(fadePanelA.DOFade(dimAlpha, fadeDuration))
            .Append(minimapGroup.DOFade(1f, minimapFadeDuration))
            .SetUpdate(true);
    }

    public void HideMinimap()
    {
        if (currentSeq != null && currentSeq.IsActive()) currentSeq.Kill();

        // 시퀀스: 미니맵 페이드아웃 → 화면 밝게
        currentSeq = DOTween.Sequence()
            .Append(minimapGroup.DOFade(0f, minimapFadeDuration))
            .Append(fadePanelA.DOFade(0f, fadeDuration))
            .OnComplete(() =>
            {
                minimapUI.gameObject.SetActive(false);
                fadePanelA.gameObject.SetActive(false);
            })
            .SetUpdate(true);
    }

    public void ResetFadePanels()
    {
        // 완전 초기 상태
        if (fadePanelA != null)
        {
            fadePanelA.gameObject.SetActive(false);
            fadePanelA.color = new Color(0f, 0f, 0f, 0f);
        }

        if (minimapUI != null)
        {
            minimapUI.gameObject.SetActive(false);
            if (minimapGroup == null) minimapGroup = minimapUI.GetComponent<CanvasGroup>();
            if (minimapGroup == null) minimapGroup = minimapUI.gameObject.AddComponent<CanvasGroup>();
            minimapGroup.alpha = 0f;
        }
    }
}