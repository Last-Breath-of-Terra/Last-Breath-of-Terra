using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHoverHandler : MonoBehaviour
{
    private Obstacle parentObstacle;

    private void Start()
    {
        parentObstacle = GetComponentInParent<Obstacle>();
    }

    private void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);
        bool isCurrentlyHovered = false;

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isCurrentlyHovered = true;
                break;
            }
        }

        parentObstacle.SetHovered(isCurrentlyHovered);
    }
}
