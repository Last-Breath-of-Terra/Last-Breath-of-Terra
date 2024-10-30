using UnityEngine;

/// <summary>
/// 장애물 데이터를 관리하는 SO 클래스
/// </summary>

[CreateAssetMenu(fileName = "NewObstacle", menuName = "ScriptableObject/Obstacle")]
public class ObstacleSO : ScriptableObject
{
    public float speed = 3f;
    public int clicksToDestroy = 3;
}