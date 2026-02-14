using UnityEngine;

public class KillPlayerObstacle : MonoBehaviour
{
    public float damage;
    public float speed;
    public LifeInfuserSO lifeInfuserSO;
    public Transform targetPoint;

    public GameObject destroyEffectPrefab;

    private bool isActive = true;
    private float currentSpeed;

    private void OnEnable()
    {
        isActive = true;
        currentSpeed = speed;

        if (targetPoint == null && GameManager.Instance != null)
            targetPoint = GameManager.Instance.playerTr;
    }

    private void OnDisable()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        if (targetPoint == null) return;

        Vector3 dir = (targetPoint.position - transform.position).normalized;
        transform.position += dir * currentSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("1");
        if (!collision.CompareTag("Player")) return;
        Debug.Log("2");
        if (lifeInfuserSO != null)
            lifeInfuserSO.StopInfusion(collision.GetComponent<AudioSource>());
        Debug.Log("3");

        GameManager.Instance._ui.StageMinimapManager.ForceCloseMap();
        Debug.Log("4");

        float playerFacing = GameManager.Instance.playerTr.localScale.x;
        Vector2 knockbackDir = playerFacing > 0 ? Vector2.left : Vector2.right;
        Debug.Log("5");

        Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
        PlayerController controller = collision.GetComponent<PlayerController>();
        Debug.Log("6");

        controller.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Knockback);
        AudioManager.Instance.PlayRandomSFX("knockback_", collision.GetComponent<AudioSource>(), transform);
        Debug.Log("7");

        playerRb.AddForce(knockbackDir * 3f, ForceMode2D.Impulse);

        controller.HP -= damage;
        Debug.Log("8");

        Invoke(nameof(ReactivatePlayerMovement), 0.5f);

        DeactivateObstacle();
    }

    private void DeactivateObstacle()
    {
        if (destroyEffectPrefab != null)
        {
            GameObject fx = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        gameObject.SetActive(false);
    }

    private void ReactivatePlayerMovement()
    {
        Debug.Log("8");

        PlayerController playerController = GameManager.Instance.playerTr.GetComponent<PlayerController>();
        playerController.SetCanMove(true);
        playerController.AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
    }
}
