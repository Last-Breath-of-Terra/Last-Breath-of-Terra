using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public WindManager.WindDirection windDirection;
    private WindManager.WindType windType;

    public float moveAmount = 0.01f;

    private Coroutine coroutine;
    private bool isToRightOfWind;
    private WindManager.WindDirection playerDirection;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            WindManager.Instance.PlayWindAudio(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            WindManager.Instance.ApplyWindEffect(windDirection, rb.velocity);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            WindManager.Instance.RemoveWindEffect(windType);
        }
    }
}