using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMinimapObjectController : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (image != null)
        {
            image.sprite = sprite;
        }
    }
}
