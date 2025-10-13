using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHoverHandler : MonoBehaviour
{
    private Obstacle parentObstacle;
    private ObstacleProjectile parentProjectile;

    private void Start()
    {
        parentObstacle = GetComponentInParent<Obstacle>();
        parentProjectile = GetComponentInParent<ObstacleProjectile>();
    }

    private void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        int hoverLayerMask = LayerMask.GetMask("obstacleHover");
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, hoverLayerMask);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            if (parentObstacle != null)
            {
                parentObstacle.SetHovered(true);
            }
            else if (parentProjectile != null)
            {
                parentProjectile.SetHovered(true);
            }
        }
        else
        {
            if (parentObstacle != null)
            {
                parentObstacle.SetHovered(false);
            }
            else if (parentProjectile != null)
            {
                parentProjectile.SetHovered(false);
            }
        }
    }
}