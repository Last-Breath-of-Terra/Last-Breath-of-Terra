using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObstacleProjectile : MonoBehaviour
{
    private Vector3 target;
    private GameObject player;
    private float speed;
    private ProjectileObstacleController obstacle;
    private InputAction clickAction;

    private bool isClicked = false;

    public void Initialize(Vector3 targetPos, GameObject playerObject, float moveSpeed, ProjectileObstacleController obstacleRef)
    {
        target = targetPos;
        player = playerObject;
        speed = moveSpeed;
        obstacle = obstacleRef;

        var playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.FindActionMap("Gimmick").Enable();
            playerInput.actions.FindActionMap("Player").Disable();

            clickAction = playerInput.actions["Projectile"];
            clickAction.performed += OnClickPerformed;
        }
    }

    void Update()
    {
        if (isClicked) return;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            Destroy(gameObject);
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnClicked();
                break;
            }
        }
    }
    

    private void OnClicked()
    {
        isClicked = true;
        player.GetComponent<PlayerController>().SetCanMove(true);
        obstacle.OnOneProjectileDestroyed();
        Destroy(gameObject);
        StartCoroutine(UnblockInput());
    }

    IEnumerator UnblockInput()
    {
        yield return new WaitForSeconds(0.5f);
        player.GetComponent<PlayerController>().SetCanMove(false);
    }

    private void OnDestroy()
    {
        if (clickAction != null)
        {
            clickAction.performed -= OnClickPerformed;
        }
    }
}