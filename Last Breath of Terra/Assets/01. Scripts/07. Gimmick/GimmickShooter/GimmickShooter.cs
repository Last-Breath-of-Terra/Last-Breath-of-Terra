using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GimmickShooter : MonoBehaviour
{
    public float shootForce = 0f;
    public bool isStage1 = true;
    private GameObject obstacle;
    private Coroutine coroutine;
    private float respawnCooldown;

    private void Start()
    {
        if (shootForce == 0f)
            shootForce = GimmickShooterManager.Instance.shootForce;
    }

    public void StartShooter()
    {
        if (isStage1)
            GimmickManager.Instance.PlayGimmickSFX("Sfx_ShootingWarning01_01", gameObject, false);
        else
            GimmickManager.Instance.PlayGimmickSFX("Sfx_IceFlowerWarning01", gameObject, true);
        respawnCooldown = Random.Range(GimmickShooterManager.Instance.respawnTimeRange.x,
            GimmickShooterManager.Instance.respawnTimeRange.y);
        Debug.Log("respawnCooldown : " + respawnCooldown);
        coroutine = StartCoroutine(MoveShooter());
    }

    public void StopShooter()
    {
        StopCoroutine(coroutine);
    }

    IEnumerator MoveShooter()
    {
        float audioLength;
        while (true)
        {
            obstacle = PoolManager.Instance.GetObject(GimmickShooterManager.Instance.poolName);
            if (obstacle != null)
            {
                obstacle.transform.position = gameObject.transform.position;

                float zRotation = gameObject.transform.eulerAngles.z;

                if (zRotation > 90f && zRotation < 270f)
                {
                    obstacle.GetComponent<SpriteRenderer>().flipX = false;
                }
                else
                {
                    obstacle.GetComponent<SpriteRenderer>().flipX = true;
                }

                Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
                audioLength = isStage1
                    ? GimmickManager.Instance.PlayGimmickSFX("Sfx_ShootingFire02", gameObject, true)
                    : GimmickManager.Instance.PlayGimmickSFX("Sfx_IceFlowerFire02", gameObject, true);
                if (isStage1)
                    rb.AddForce(gameObject.transform.up.normalized * GimmickShooterManager.Instance.shootForce,
                        ForceMode2D.Impulse);
                else
                    rb.AddForce(gameObject.transform.right.normalized * GimmickShooterManager.Instance.shootForce,
                        ForceMode2D.Impulse);
            }

            yield return new WaitForSeconds(respawnCooldown);
            audioLength = isStage1
                ? GimmickManager.Instance.PlayGimmickSFX("Sfx_ShootingWarning01_01", gameObject, false)
                : GimmickManager.Instance.PlayGimmickSFX("Sfx_IceFlowerWarning01", gameObject, true);
            yield return new WaitForSeconds(audioLength);

            PoolManager.Instance.ReturnObject(GimmickShooterManager.Instance.poolName, obstacle);
        }
    }
}