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
            float moveTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_Windlift01", gameObject, false);

            _tween = playerTransform.DOMoveY(playerTransform.position.y + WindManager.Instance.windSO.liftHeight, moveTime).SetEase(Ease.OutSine);;
            yield return new WaitForSeconds(moveTime);
            moveTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_windhover02", gameObject, false);
            yield return new WaitForSeconds(moveTime);
            _tween.Kill();
            moveTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_windfall03", gameObject, false);
            _tween = playerTransform.DOMoveY(playerTransform.position.y - WindManager.Instance.windSO.liftHeight, WindManager.Instance.windSO.deactivationTime).SetEase(Ease.InSine);;
            yield return new WaitForSeconds(moveTime);
            yield return new WaitForSeconds(WindManager.Instance.windSO.deactivationTime - moveTime);

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
