using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController controller;
    private PlayerInput input;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        input = GetComponent<PlayerInput>();

        moveAction = input.actions["Move"];
        jumpAction = input.actions["Jump"];
        attackAction = input.actions["Attack"];
    }

    void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJumpPerformed;
        attackAction.performed += OnAttackPerformed;
    }

    void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJumpPerformed;
        attackAction.performed -= OnAttackPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (controller.canMove)
            Invoke(nameof(StartMoving), 0.3f);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        controller.Movement.StopMoving();
        GameManager.Instance._ui.ReleaseClick();
        controller.AudioHandler.StopCurrentCancelable();
        controller.AudioHandler.PlayLandingSound();
        CancelInvoke(nameof(StartMoving));
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!controller.canMove) return;

        if (controller.WallClimb.IsClimbing())
            controller.WallClimb.JumpFromWall();
        else
            controller.Movement.TryJump();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        controller.Movement.TryAttack();
    }

    private void StartMoving()
    {
        controller.Movement.StartMoving();
    }
}