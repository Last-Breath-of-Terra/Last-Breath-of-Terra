using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpikeTrapTriggerArea : MonoBehaviour
{
    public float spikeActiveTime;
    public float cooldownTime;

    private Transform trap;
    private Coroutine coroutine;

    private void Start()
    {
        trap = transform.GetChild(0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        coroutine = StartCoroutine(MoveTrap());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StopCoroutine(coroutine);
    }

    IEnumerator MoveTrap()
    {
        while (true)
        {
            trap.DOKill();
            trap.DOMoveY(transform.position.y + 2f, 1f)
                .SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(spikeActiveTime);
            trap.DOKill();
            trap.DOMoveY(transform.position.y - 2f, 1f)
                .SetEase(Ease.InQuad);
            yield return new WaitForSeconds(cooldownTime);
        }
    }
}