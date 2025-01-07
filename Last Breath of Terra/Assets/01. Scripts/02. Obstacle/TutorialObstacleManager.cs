using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialObstacleManager : MonoBehaviour
{
    public GameObject tutorialObstaclePrefab;
    public List<Transform> spawnPositions;
    private List<TutorialObstacle> tutorialObstacles = new List<TutorialObstacle>();

    private void Start()
    {
        SpawnTutorialObstacles();
    }

    private void SpawnTutorialObstacles()
    {
        foreach (Transform spawnPoint in spawnPositions)
        {
            Vector3 position = spawnPoint.position;
            GameObject obj = Instantiate(tutorialObstaclePrefab, position, Quaternion.identity);
            TutorialObstacle obstacle = obj.GetComponent<TutorialObstacle>();
            tutorialObstacles.Add(obstacle);

            obstacle.OnObstacleDisabled += HandleObstacleDisabled;
        }
    }

    private void HandleObstacleDisabled(TutorialObstacle obstacle)
    {
        StartCoroutine(ReactivateObstacleAfterDelay(obstacle, 5f));
    }

    private IEnumerator ReactivateObstacleAfterDelay(TutorialObstacle obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obstacle != null)
        {
            obstacle.ReactivateObstacle(obstacle.transform.position);
        }
    }

    public void ResetAllObstacles()
    {
        foreach (var obstacle in tutorialObstacles)
        {
            if (!obstacle.gameObject.activeSelf)
            {
                obstacle.ReactivateObstacle(obstacle.transform.position);
            }
        }
    }
}
