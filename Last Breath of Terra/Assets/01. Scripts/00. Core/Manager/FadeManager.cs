using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
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
}
