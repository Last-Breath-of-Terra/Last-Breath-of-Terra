using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UpwardWind : MonoBehaviour
{
    
    private Coroutine _coroutine;
    private Tween _tween;
    
    IEnumerator StartUpwardWind(Transform playerTransform)
    {
        while (true)
        {
            _tween.Kill();
            _tween = playerTransform.DOMoveY(playerTransform.position.y + WindManager.Instance.windSO.liftHeight, WindManager.Instance.windSO.activationTime).SetEase(Ease.OutSine);;
            yield return new WaitForSeconds(WindManager.Instance.windSO.activationTime);
            _tween.Kill();
            _tween = playerTransform.DOMoveY(playerTransform.position.y - WindManager.Instance.windSO.liftHeight, WindManager.Instance.windSO.deactivationTime).SetEase(Ease.InSine);;
            yield return new WaitForSeconds(WindManager.Instance.windSO.deactivationTime);

        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        _coroutine = StartCoroutine(StartUpwardWind(other.gameObject.transform));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _tween.Kill();
        StopCoroutine(_coroutine);
    }
}
