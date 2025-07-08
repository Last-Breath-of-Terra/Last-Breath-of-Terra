using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceDevilController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;
    public float moveDisableDuration = 1f;
    public float damage = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.hp -= damage;

        Vector2 knockbackDir = (player.transform.position - transform.position).normalized;

        Rigidbody2D rb = player.Rb;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        player.SetCanMove(false);
        player.StartCoroutine(EnablePlayerMovementAfterDelay(player));
    }

    private IEnumerator EnablePlayerMovementAfterDelay(PlayerController player)
    {
        yield return new WaitForSeconds(moveDisableDuration);
        player.SetCanMove(true);
    }
}
