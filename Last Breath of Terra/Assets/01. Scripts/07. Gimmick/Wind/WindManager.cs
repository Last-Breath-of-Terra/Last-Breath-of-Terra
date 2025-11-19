using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        Slow
    }


    public WindSO windSO;
    private PlayerMovement player;
    private Coroutine _coroutine;
    private GameObject windObject;



    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void ApplyWindEffect(WindType windType)
    {
        float drag = 1f;
        switch (windType)
        {
            case WindType.Fast:
                drag = windSO.fastRate;
                break;
            case WindType.Slow:
                drag = windSO.slowRate;
                break;
            default:
                drag = 1f;
                break;
        }

        player.SpeedChangeRate = drag;
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