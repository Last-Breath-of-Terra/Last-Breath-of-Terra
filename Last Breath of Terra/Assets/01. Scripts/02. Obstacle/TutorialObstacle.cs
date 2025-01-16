using System;
using UnityEngine;

/// <summary>
/// 튜토리얼 전용 장애물 클래스
/// </summary>
public class TutorialObstacle : Obstacle
{
    public event Action<TutorialObstacle> OnObstacleDisabled;

    private void Update()
    {
        if (isHovered && isRotating)
        {
            RotateTimingIndicator();
        }
    }

    protected override void DeactivateObstacle()
    {
        GameObject effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effect, 2f);
        
        gameObject.SetActive(false);
        OnObstacleDisabled?.Invoke(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isTrigger)
        {
            isHovered = false;
            HandleHoverEffect();
        }
    }
}
