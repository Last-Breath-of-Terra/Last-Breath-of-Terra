using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;


public class LifeInfuser : MonoBehaviour
{
    public LifeInfuserSO lifeInfuserData;
    public Transform[] obstacleSpawnPoints;
    public int infuserNumber;
    public int infuserType;
    public float spawnDelay = 1f;
    public float spawnInterval = 2f;
    public Transform arrivalPoint;
    //public bool[] canInfusion;
    
    private Tween startTween;
    private Coroutine obstacleSpawnCoroutine;
    private PlayerController _playerController;
    private Material mat;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat("_Enabled", 0f);
        mat.SetFloat("_Thickness", 0f);
        Debug.Log(InfuserManager.Instance.gameObject.name);

        InfuserManager.Instance.canInfusion[infuserNumber] = true;
        InfuserManager.Instance.infuser[infuserNumber] = gameObject;
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player") && InfuserManager.Instance.canInfusion[infuserNumber])
        {
            _playerController = collision.GetComponent<PlayerController>();
            startTween = DOVirtual.DelayedCall(lifeInfuserData.infusionWaitTime, () =>
            {
                PrepareInfusion();
            });

            GameManager.Instance._shaderManager.TurnOnOutline(mat, 3f, 0.5f);
        }
    }
    private void PrepareInfusion()
    {
        Debug.Log("Prepare Infusion");

        if (_playerController != null)
        {
            AudioManager.instance.PlaySFX("breath_action_start", gameObject.GetComponent<AudioSource>(), gameObject.transform);
            _playerController.SetActivatingState(true);
            _playerController.SetCanMove(false);
        }
        DOTween.To(() => lifeInfuserData.defaultLensSize, x => InfuserManager.Instance.virtualCamera.m_Lens.OrthographicSize = x, lifeInfuserData.targetLensSize, 1f);
        lifeInfuserData.StartInfusion(infuserNumber, gameObject);
        
        if (obstacleSpawnCoroutine == null)
        {
            if(GameManager.ScenesManager.GetCurrentSceneType().ToString() != "Tutorial")
                obstacleSpawnCoroutine = StartCoroutine(SpawnObstaclesPeriodically());
        }
        
        GameManager.Instance._shaderManager.PlayInfusionSequence(
            mat, 
            Camera.main.GetComponent<Volume>(), 
            lifeInfuserData.infusionDuration, 
            CompleteInfusion
        );
    }

    private IEnumerator SpawnObstaclesPeriodically()
    {
        yield return new WaitForSeconds(spawnDelay);

        while (true)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnObstacle()
    {
        if (obstacleSpawnPoints == null || obstacleSpawnPoints.Length == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, obstacleSpawnPoints.Length);
        Transform spawnPoint = obstacleSpawnPoints[randomIndex];

        Obstacle obstacle = GameManager.Instance._obstacleManager.GetObstacle();
        if (obstacle != null)
        {
            obstacle.transform.position = spawnPoint.position;
            obstacle.targetPoint = arrivalPoint;
            obstacle.ReactivateObstacle(spawnPoint.position);
        }
    }

    private void CompleteInfusion()
    {
        GameManager.Instance._shaderManager.CompleteInfusionEffect(
        mat,
        Camera.main.GetComponent<Volume>(),
        () =>
        {
            // Infusion 완료 후 처리
            lifeInfuserData.CompleteInfusion(infuserNumber, gameObject, infuserType);
            InfuserManager.Instance.canInfusion[infuserNumber] = false;
            

            if (_playerController != null)
            {
                _playerController.SetActivatingState(false);
                _playerController.SetCanMove(true);
            }

            if (obstacleSpawnCoroutine != null)
            {
                StopCoroutine(obstacleSpawnCoroutine);
                obstacleSpawnCoroutine = null;
            }
        });
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (startTween != null)
        {
            startTween.Kill();
        }
        lifeInfuserData.StopInfusion(gameObject.GetComponent<AudioSource>());
        if (_playerController != null)
        {
            _playerController.SetCanMove(true);
        }

        GameManager.Instance._shaderManager.TurnOffOutline(mat, 0.5f);
    }
}
