using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public enum BackgroundType
{
    A,
    B,
    C,
    D
}

public class StoryController : MonoBehaviour
{
    public Image fadeImage;
    public Image background;

    [System.Serializable]
    public struct BackgroundTextPair
    {
        public Sprite backgroundImage;
        public string[] sentences;
        public BackgroundType backgroundType;
    }

    public BackgroundTextPair[] backgroundTextPairs;

    [Header("Background Setting")] 
    public float moveDuration = 15f;

    [Header("Text Setting")] 
    public Transform textParent;
    public GameObject textLinePrefab;
    public float textDisplayDuration = 2f;
    public float waitBetweenText = 1f;

    [Header("Scene Setting")]
    public float fadeDuration = 1f;
    public float sceneWaitTime = 2f;
    public string nextSceneName;
    
    private Vector2 moveDirection;

    private void Start()
    {
        StartCoroutine(RunSceneSequence());
    }

    private IEnumerator RunSceneSequence()
    {
        foreach (var pair in backgroundTextPairs)
        {
            SetBackgroundInfo(pair);

            Coroutine backgroundMoveCoroutine = StartCoroutine(MoveBackground(background, moveDirection));

            yield return StartCoroutine(FadeIn());
            yield return StartCoroutine(DisplayText(pair.sentences, pair.backgroundType));
            yield return new WaitForSeconds(sceneWaitTime);
            yield return StartCoroutine(FadeOut());

            StopCoroutine(backgroundMoveCoroutine);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 1f;
        fadeImage.color = startColor;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            startColor.a = Mathf.Clamp01(1 - time / fadeDuration);
            fadeImage.color = startColor;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 0f;
        fadeImage.color = startColor;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            startColor.a = Mathf.Clamp01(time / fadeDuration);
            fadeImage.color = startColor;
            yield return null;
        }
    }

    private IEnumerator PlayBackgroundAndText(string[] sentences, BackgroundType backgroundType)
    {
        var backgroundMove = StartCoroutine(MoveBackground(background, moveDirection));
        var textDisplay = StartCoroutine(DisplayText(sentences, backgroundType));
        
        yield return backgroundMove;
        yield return textDisplay;
    }

    private IEnumerator MoveBackground(Image background, Vector2 moveDirection)
    {
        RectTransform rt = background.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + moveDirection;
        float time = 0f;

        while (time < moveDuration)
        {
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, time / moveDuration);
            time += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = endPos;
    }

    private IEnumerator DisplayText(string[] sentences, BackgroundType backgroundType)
    {
        if (sentences.Length == 0)
        {
            yield break;
        }

        // 정렬/위치 설정을 위한 기준값
        TextAlignmentOptions alignment = TextAlignmentOptions.Left;
        Vector2 anchoredPos = new Vector2(-350f, 0f);
        TextAnchor layoutGroupAlign = TextAnchor.UpperLeft;

        if (backgroundType == BackgroundType.B || backgroundType == BackgroundType.D)
        {
            alignment = TextAlignmentOptions.Right;
            anchoredPos = new Vector2(-10f, 0f);
            layoutGroupAlign = TextAnchor.UpperRight;
        }

        var layoutGroup = textParent.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = layoutGroupAlign;

        foreach (string line in sentences)
        {
            // 텍스트 오브젝트 생성
            GameObject lineObj = Instantiate(textLinePrefab, textParent);
            Transform wrapperTr = lineObj.transform.Find("TextWrapper");
            TextMeshProUGUI tmp = wrapperTr.GetComponent<TextMeshProUGUI>();
            RectTransform wrapperRT = wrapperTr.GetComponent<RectTransform>();

            // 정렬 설정
            tmp.alignment = alignment;
            wrapperRT.anchoredPosition = anchoredPos;

            // 알파 점진적으로 증가
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime / textDisplayDuration;
                alpha = Mathf.Clamp01(alpha);

                Color c = tmp.color;
                c.a = alpha;
                tmp.color = c;

                tmp.text = line;
                yield return null;
            }

            tmp.text = line;
            yield return new WaitForSeconds(waitBetweenText);
        }
    }

    private void SetBackgroundInfo(BackgroundTextPair pair)
    {
        background.sprite = pair.backgroundImage;
        foreach (Transform child in textParent)
        {
            Destroy(child.gameObject);
        }
        RectTransform rt = background.GetComponent<RectTransform>();
        
        switch (pair.backgroundType)
        {
            case BackgroundType.A:
                rt.sizeDelta = new Vector2(2880, 1080);
                moveDirection = new Vector2(-950, 0);
                rt.anchoredPosition = new Vector2(-480, 0);
                break;
            case BackgroundType.B:
                rt.sizeDelta = new Vector2(2880, 1080);
                GameObject.Find("Canvas").transform.Find("BackImage").transform.rotation = Quaternion.Euler(0, 0, 180);
                moveDirection = new Vector2(940, 0);
                rt.anchoredPosition = new Vector2(-1430, 0);
                break;
            case BackgroundType.C:
                rt.sizeDelta = new Vector2(1920, 1620);
                moveDirection = new Vector2(0, -540);
                rt.anchoredPosition = new Vector2(-960, 270);
                break;
            case BackgroundType.D:
                rt.sizeDelta = new Vector2(1920, 1620);
                moveDirection = new Vector2(0, 540);
                rt.anchoredPosition = new Vector2(-960, -270);
                break;
        }
    }
}
