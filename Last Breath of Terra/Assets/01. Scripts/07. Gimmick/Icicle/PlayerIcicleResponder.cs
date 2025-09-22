using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerIcicleResponder : MonoBehaviour
{
    public int unfreezeActionCount;
    public int unfreezeThreshold;
    public float freezeDuration = 5f;
    public Sprite freezedSprite;
    public ParticleSystem freezedParticles;
    private PlayerInput playerInput;
    private Coroutine freezeCoroutine;
    private ParticleSystem particleSystem;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        unfreezeActionCount = 0;
        particleSystem = new ParticleSystem();
        particleSystem = freezedParticles;
        particleSystem.Stop();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Gimmick/Icicle"].performed += OnClickPlayer;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Gimmick/Icicle"].performed -= OnClickPlayer;
        }
    }

    public void FreezePlayer()
    {
        Debug.Log("Freezing player");
        gameObject.GetComponent<Animator>().SetBool("IcicleHit", true);
        Cursor.visible = true;
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Gimmick");

        unfreezeActionCount = 0;
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        freezeCoroutine = StartCoroutine(UnfreezePlayer());
    }

    IEnumerator UnfreezePlayer()
    {
        yield return new WaitForSeconds(freezeDuration);
        playerInput.SwitchCurrentActionMap("Player");
        gameObject.GetComponent<Animator>().SetBool("IcicleHit", false);
        Cursor.visible = false;
        GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimickiceclefallbreakfree", gameObject, true);
    }

    public void OnClickPlayer(InputAction.CallbackContext context)
    {
        //particleSystem.transform.position = transform.position;
        //particleSystem.Play();
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        LayerMask mask = LayerMask.GetMask("Player");
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, mask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                unfreezeActionCount++;
                if (unfreezeActionCount >= unfreezeThreshold)
                {
                    playerInput.SwitchCurrentActionMap("Player");
                    Cursor.visible = false;
                    gameObject.GetComponent<Animator>().SetBool("IcicleHit", false);

                    StopCoroutine(freezeCoroutine);
                }
            }
        }
    }
}