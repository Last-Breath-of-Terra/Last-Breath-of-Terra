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

        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        bool isCurrentlyHovered = hit.collider != null && hit.collider.gameObject == gameObject;

        parentObstacle.SetHovered(isCurrentlyHovered);
    }
}
