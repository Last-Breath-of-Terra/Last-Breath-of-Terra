using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutAndDestroy : MonoBehaviour
{
    public float fadeOutTimer;
    SpriteRenderer targetRenderer;
    

    void Start()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Color fade = targetRenderer.color;
        fade.a = Mathf.MoveTowards(fade.a, 0, 1/fadeOutTimer * Time.deltaTime);
        targetRenderer.color = fade;


        if (fade.a <= 0)
            gameObject.SetActive(false);
    }
}
