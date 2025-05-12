using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Icicle : MonoBehaviour
{
    public int demege;
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

    private void Update()
    {
        gameObject.transform.position += fallSpeed * Time.deltaTime * Vector3.down ;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().hp -= demege;
        }
        PoolManager.Instance.ReturnObject(IcicleManager.Instance.poolName, gameObject);

        /*
        if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
            
        }
        */
    }
    
    public void Fallicicle()
    {
        DOTween.To(() => fallSpeed, x => fallSpeed = x, maxFallSpeed, fallDuration)
            .SetEase(Ease.InQuad); 
    }

}
