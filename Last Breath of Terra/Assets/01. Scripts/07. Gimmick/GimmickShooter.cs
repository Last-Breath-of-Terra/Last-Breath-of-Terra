using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GimmickShooter : MonoBehaviour
{
    private GimmickShooterController gimmickShooterController;
    private GameObject obstacle;
    private Coroutine coroutine;
    private float respawnCooldown;

    public void Initialize(GimmickShooterController controller)
    {
        gimmickShooterController = controller;
    }

    public void StartShooter()
    {
        respawnCooldown = Random.Range(gimmickShooterController.respawnTimeRange.x,
            gimmickShooterController.respawnTimeRange.y);
        Debug.Log("respawnCooldown : " + respawnCooldown);
        coroutine = StartCoroutine(MoveShooter());
    }

    public void StopShooter()
    {
        StopCoroutine(coroutine);
    }

    IEnumerator MoveShooter()
    {
        while (true)
        {
            obstacle = PoolManager.Instance.GetObject(gimmickShooterController.poolName);
            obstacle.transform.position = gameObject.transform.position;
            if (obstacle != null)
            {
                Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
                rb.AddForce(gameObject.transform.right.normalized * gimmickShooterController.shootForce, ForceMode2D.Impulse);
                    /*
                obstacle.transform.DOMoveX(obstacle.transform.position.x - 10f / respawnCooldown, respawnCooldown)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        obstacle.transform.SetParent(null);
                        PoolManager.Instance.ReturnObject(gimmickShooterController.poolName, obstacle);
                    });*/
            }
            yield return new WaitForSeconds(respawnCooldown);
            PoolManager.Instance.ReturnObject(gimmickShooterController.poolName, obstacle);

        }
    }

}