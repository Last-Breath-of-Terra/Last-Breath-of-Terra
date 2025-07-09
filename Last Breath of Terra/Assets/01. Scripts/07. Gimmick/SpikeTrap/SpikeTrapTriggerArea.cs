using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpikeTrapTriggerArea : MonoBehaviour
{
    

    private Transform trap;
    private Coroutine coroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        trap = PoolManager.Instance.GetObject(SpikeTrapManager.Instance.poolName).GetComponent<Transform>();
        trap.position = gameObject.transform.position;
        coroutine = StartCoroutine(MoveTrap());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StopCoroutine(coroutine);
        PoolManager.Instance.ReturnObject(SpikeTrapManager.Instance.poolName, trap.gameObject);
    }

    IEnumerator MoveTrap()
    {
        while (true)
        {
            if (trap != null)
            {
                trap.DOKill();
                trap.DOMoveY(transform.position.y + 2f, 1f)
                    .SetEase(Ease.OutQuad);
                yield return new WaitForSeconds(SpikeTrapManager.Instance.spikeActiveTime);
                trap.DOKill();
                trap.DOMoveY(transform.position.y - 2f, 1f)
                    .SetEase(Ease.InQuad);
                yield return new WaitForSeconds(SpikeTrapManager.Instance.cooldownTime);
            }
        }
    }
}