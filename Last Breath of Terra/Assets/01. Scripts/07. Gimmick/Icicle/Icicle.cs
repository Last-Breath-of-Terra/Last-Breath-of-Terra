using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Icicle : MonoBehaviour
{
    private float fallSpeed;

    [SerializeField] private float fallDuration = 1f;
    [SerializeField] private float maxFallSpeed = 20f;

    private void Start()
    {
        fallSpeed = 0f;
        Sequence seq = DOTween.Sequence();
        transform.DOShakeRotation(
            duration: 0.3f,
            strength: 15f,
            vibrato: 10,
            randomness: 90f,
            fadeOut: false
        );
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() => Fallicicle());
        Invoke("Fallicicle", 0.2f);
    }

    private void OnEnable()
    {
        GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_iciclefallnotice_01", gameObject, true);
    }

    private void Update()
    {
        //  gameObject.transform.position += fallSpeed * Time.deltaTime * Vector3.down ;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        float audioTime = 1f;
        if (other.gameObject.CompareTag("Player"))
        {
            audioTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_iceclefallhit_04", gameObject, true);
            other.gameObject.GetComponent<PlayerIcicleResponder>().FreezePlayer();
            Invoke("Returnicicle", audioTime);

        }

        if (other.gameObject.CompareTag("Ground"))
        {
            audioTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_iceclefallhit_04", gameObject, true);
            Invoke("Returnicicle", audioTime);

           // audioTime = GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_iceclefallground03", gameObject, true);
        }
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {

        }*/
    }

    public void Fallicicle()
    {
        GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimick_iceclefall02", gameObject, true);
        DOTween.To(() => fallSpeed, x => fallSpeed = x, maxFallSpeed, fallDuration)
            .SetEase(Ease.InQuad);
    }

    private void Returnicicle()
    {
        PoolManager.Instance.ReturnObject(IcicleManager.Instance.poolName, gameObject);

    }
}