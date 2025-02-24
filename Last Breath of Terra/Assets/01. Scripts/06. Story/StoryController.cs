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
    public TMP_Text text;

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
            yield return StartCoroutine(FadeIn());

            yield return StartCoroutine(PlayBackgroundAndText(pair.sentences, pair.backgroundType));

            yield return new WaitForSeconds(sceneWaitTime);
            yield return StartCoroutine(FadeOut());
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

        RectTransform rt = text.GetComponent<RectTransform>();

        if (backgroundType == BackgroundType.A || backgroundType == BackgroundType.C)
        {
            rt.anchoredPosition = new Vector2(-600f, rt.anchoredPosition.y);
            text.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            rt.anchoredPosition = new Vector2(-10f, rt.anchoredPosition.y);
            text.alignment = TextAlignmentOptions.Right;
        }

        string m_show = "";
        foreach (string line in sentences)
        {
            float m_alphaFloat = 0.1f;

            while (m_alphaFloat < 1.0f)
            {
                m_alphaFloat += Time.deltaTime / textDisplayDuration;
                m_alphaFloat = Mathf.Clamp01(m_alphaFloat);

                Color textColor = Color.white;
                textColor.a = m_alphaFloat;

                text.text = m_show + $"<color=#{ColorUtility.ToHtmlStringRGBA(textColor)}>{line}</color>";

                yield return null;
            }

            m_show += line + "\n";

            yield return new WaitForSeconds(waitBetweenText);
        }
    }

    private void SetBackgroundInfo(BackgroundTextPair pair)
    {
        background.sprite = pair.backgroundImage;
        text.text = "";
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
