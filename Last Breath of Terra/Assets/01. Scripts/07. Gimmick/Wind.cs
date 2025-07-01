using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public WindManager.WindDirection windDirection;
    private WindManager.WindType windType;

    public float moveAmount = 0.01f;
    public float duration = 0.1f;

    private Coroutine coroutine;
    private bool isToRightOfWind;
    private WindManager.WindDirection playerDirection;


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            int velocity = rb.velocity.x > 0 ? 1 : -1;
            if ((int)windDirection == velocity)
            {
                WindManager.Instance.ApplyWindEffect(WindManager.WindType.Fast);

                //왼쪽
                // player.transform.position = new Vector3(playerX + moveAmount, player.transform.position.y, player.transform.position.z);
            }
            else
            {
                WindManager.Instance.ApplyWindEffect(WindManager.WindType.Slow);

                //오른쪽
                // player.transform.position = new Vector3(playerX + moveAmount, player.transform.position.y, player.transform.position.z);
            }

            rb.AddForce(moveAmount * (int)windDirection * Vector2.right);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            WindManager.Instance.RemoveWindEffect(windType);
        }
    }
}