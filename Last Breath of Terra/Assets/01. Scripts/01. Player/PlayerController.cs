using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerWallClimb))]
[RequireComponent(typeof(PlayerAnimationHandler))]
[RequireComponent(typeof(PlayerAudioHandler))]
public class PlayerController : MonoBehaviour
{
    public PlayerSO data;

    [Header("Movement Settings")]
    public Transform MovementGroundCheckPoint;
    public LayerMask MovementGroundLayer;

    [HideInInspector] public Rigidbody2D Rb;
    [HideInInspector] public Vector3 OriginalScale;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public float hp;

    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerWallClimb WallClimb { get; private set; }
    public PlayerAnimationHandler AnimHandler { get; private set; }
    public PlayerAudioHandler AudioHandler { get; private set; }

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        OriginalScale = transform.localScale;
        hp = data.hp;

        InputHandler = GetComponent<PlayerInputHandler>();
        Movement = GetComponent<PlayerMovement>();
        WallClimb = GetComponent<PlayerWallClimb>();
        AnimHandler = GetComponent<PlayerAnimationHandler>();
        AudioHandler = GetComponent<PlayerAudioHandler>();
    }

    void Update()
    {
        Movement.HandleUpdate();
        WallClimb.HandleUpdate();
        AnimHandler.UpdateSpeedParameter(Movement.GetCurrentSpeed());
    }

    void FixedUpdate()
    {
        Movement.HandleFixedUpdate();
        WallClimb.HandleFixedUpdate();
    }

    public void SetCanMove(bool value)
    {
        if (!value)
        {
            GameManager.Instance._ui.ReleaseClick();
        }
        canMove = value;
    }

    public void SetActivatingState(bool isActivating)
    {
        AnimHandler.ChangeState(
            isActivating ? PlayerAnimationHandler.AnimationState.Activating
                         : PlayerAnimationHandler.AnimationState.Idle
        );
    }

    public void SetKnockdownState(bool isKnockdown)
    {
        AnimHandler.ChangeState(
            isKnockdown ? PlayerAnimationHandler.AnimationState.Knockdown
                        : PlayerAnimationHandler.AnimationState.Idle
        );
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.point.y < transform.position.y)
                {
                    if (Movement.IsSignificantFall())
                    {
                        StartCoroutine(Movement.HandleLandingDelayExternally());
                    }

                    AudioHandler.PlayLandingSound();
                    AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
                }
            }
        }

        if (collision.gameObject.CompareTag("Wall") && !WallClimb.IsOnWall())
        {
            WallClimb.StickToWall();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !WallClimb.IsWallJumping())
        {
            WallClimb.FallOffWall();
        }
    }
} 