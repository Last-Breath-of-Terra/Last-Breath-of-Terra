using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudioHandler : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerController controller;
    private float footstepTimer;
    private readonly float footstepInterval = 0.5f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<PlayerController>();
    }

    public void HandleFootstepSound()
    {
        if (!controller.Movement.IsGrounded() || controller.Rb.velocity.magnitude < 0.1f) return;

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            AudioManager.Instance.PlayRandomPlayer(GetFootstepClipPrefix(), 0);
        }
    }

    public void PlayLandingSound()
    {
        AudioManager.Instance.PlaySFX(GetFootstepClipPrefix() + "4", audioSource, transform);
    }

    public void StopCurrentCancelable()
    {
        AudioManager.Instance.StopCancelable(audioSource);
    }

    public string GetFootstepClipPrefix()
    {
        return "footstep_" + GameManager.ScenesManager.GetCurrentSceneType() + "_";
    }
}