using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WindManager : Singleton<WindManager>
{
    public enum WindDirection
    {
        Left = -1,
        Right = 1
    }

    public enum WindType
    {
        Fast,
        Slow,
    }


    public WindSO windSO;
    private PlayerMovement player;
    private Coroutine _coroutine;
    private GameObject windObject;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }


    public void ApplyWindEffect(WindDirection windDirection, Vector2 velocity)
    {
        if (velocity.magnitude > 0)
        {
            float drag = 0 > velocity.x ? windSO.fastRate : windSO.slowRate;
            player.SpeedChangeRate = drag;
        }
        else
        {
            player.transform.DOMoveX(
                player.transform.position.x + (int)windDirection * 0.3f, 0.5f, false);
        }
    }

    public void RemoveWindEffect(WindType windType)
    {
        StopCoroutine(_coroutine);
        GimmickManager.Instance.StopGimmickSFX();
        GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_windpushend_03", windObject, false);
        player.SpeedChangeRate = 1f;
    }

    public void PlayWindAudio(GameObject gameObject)
    {
        windObject = gameObject;
        _coroutine = StartCoroutine(PlayWindAudioCoroutine());
    }

    IEnumerator PlayWindAudioCoroutine()
    {
        while (true)
        {
            float audioLength = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_windpushloop_02", windObject, false);
            yield return new WaitForSeconds(audioLength);
        }
    }
}