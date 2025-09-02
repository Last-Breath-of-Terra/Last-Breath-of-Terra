using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Stone : MonoBehaviour
{
    public float moveDisableDuration = 1f;
    public float knockbackForce = 10f;
    public float slowDownDuration = 5f;

    private float fallSpeed;

    [SerializeField] private float fallDuration = 1f;
    [SerializeField] private float maxFallSpeed = 20f;
    private Sequence seq;


    private void Start()
    {
        fallSpeed = 0f;

        seq = DOTween.Sequence();
        transform.DOShakeRotation(
            duration: 0.3f,
            strength: 15f,
            vibrato: 10,
            randomness: 90f,
            fadeOut: false
        );
        seq.AppendInterval(0.2f).AppendCallback(() => FallStone());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GimmickManager.Instance.ChangeLifeInfuserUISize();

            PlayerController player = other.GetComponent<PlayerController>();
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (player == null || playerMovement == null) return;

            //이동 속도 변화
            if (seq != null && seq.IsActive())
            {
                seq.Kill();
                player.SetCanMove(true);
                playerMovement.SpeedChangeRate = 1f;
            }

            player.SetCanMove(false);
            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            Rigidbody2D rb = player.Rb;
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

            seq = DOTween.Sequence();
            seq.AppendInterval(moveDisableDuration).AppendCallback(() =>
                {
                    player.SetCanMove(true);
                    Debug.Log("Can move");
                    playerMovement.SpeedChangeRate = 0.5f;
                })
                .AppendInterval(slowDownDuration).AppendCallback(() => playerMovement.SpeedChangeRate = 1f);
        }

        if (other.gameObject.CompareTag("Ground"))
        {
            Debug.Log("충돌 : " + other.gameObject.name);
            PoolManager.Instance.ReturnObject(IcicleManager.Instance.poolName, gameObject);
        }
    }

    public void FallStone()
    {
        DOTween.To(() => fallSpeed, x => fallSpeed = x, maxFallSpeed, fallDuration)
            .SetEase(Ease.InQuad);
    }

    IEnumerator ChangeMoveSpeed(GameObject player)
    {
        yield return new WaitForSeconds(moveDisableDuration);
        Debug.Log("속도가 안돌아오는 이유가 뭘까");
        player.GetComponent<PlayerController>().SetCanMove(true);
        player.GetComponent<PlayerMovement>().SpeedChangeRate = 1f;
    }
}