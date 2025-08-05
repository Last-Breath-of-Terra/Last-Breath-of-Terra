using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class MiniMapUIController : MonoBehaviour
{
    public Image fadePanelA;
    public RectTransform fadePanelB;
    public RectTransform minimapUI;

    private float screenWidth;

    private void Awake()
    {
        screenWidth = Screen.width;
    }

    public void ShowMinimap()
    {
        StartCoroutine(PlayOpenAnimation());
    }

    public void ResetFadePanels()
    {
        fadePanelA.color = new Color(0f, 0f, 0f, 0f);

        fadePanelB.anchoredPosition = new Vector2(0f, 0f);
        fadePanelB.gameObject.SetActive(false);
        minimapUI.gameObject.SetActive(false);
    }

    private IEnumerator PlayOpenAnimation()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(fadePanelA.DOFade(0.7f, 0.5f));
        seq.AppendCallback(() =>
        {
            fadePanelB.gameObject.SetActive(true);
            minimapUI.gameObject.SetActive(true);
        });
        seq.Append(fadePanelB.DOAnchorPos(new Vector2(screenWidth+500, 0f), 0.7f).SetEase(Ease.OutSine));

        seq.OnComplete(() =>
        {
            fadePanelB.gameObject.SetActive(false);
        });

        yield return seq.WaitForCompletion();
    }
}