using UnityEngine;

/// <summary>
/// 적 데이터를 관리하는 SO 클래스
/// </summary>

[CreateAssetMenu(fileName = "NewEnemy", menuName = "ScriptableObjects/Enemy")]
public class EnemySO : ScriptableObject
{
    public float speed = 3f;
    public int clicksToDestroy = 3;
}