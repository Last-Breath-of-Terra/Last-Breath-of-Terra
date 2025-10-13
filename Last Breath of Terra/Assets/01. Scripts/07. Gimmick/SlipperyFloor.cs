using UnityEngine;

public class SlipperyFloor : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.Movement.EnterSliding();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.Movement.ExitSliding();
        }
    }
}
