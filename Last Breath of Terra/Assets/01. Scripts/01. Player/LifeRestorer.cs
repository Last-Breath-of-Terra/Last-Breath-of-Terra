using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class LifeRestorer : MonoBehaviour
{
    public LifeInfuserSO lifeInfuserData;
    public CinemachineVirtualCamera camera;
    public CinemachineVirtualCamera infuserTrackedCamera;
    public ObstacleManager obstacleManager;

    [SerializeField] private InputActionMap selectMap;

    private PlayerInput playerInput;
    private Vector2 defaultSize = new Vector2(174, 271);
    private Vector2 newSize;
    private int infuserNumber;

    private bool isBigger = false;
    private float uiTweenDuration = 2f; // 애니메이션을 2초 동안 실행

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        InfuserManager.Instance.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        newSize = defaultSize * 1.2f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Obstacle"))
        {
            var playerController = GetComponent<PlayerController>();

            if (!InfuserManager.Instance.activatedInfusers.Any(active => active))
            {
                SceneManager.LoadScene("StageSelection");
            }
            else if (playerController.HP <= 0)
            {
                playerController.SetKnockdownState(true);
                obstacleManager.isRestoring = true;
                obstacleManager.StopAllObstacles();
                Invoke(nameof(StartLifeRestorer), 1f);
            }
        }
    }

    private void StartLifeRestorer()
    {
        Debug.Log("Start Life Restorer");

        // 입력 모드 전환
        SwitchActionMap("Select");
        infuserNumber = 0;

        // UI 활성화
        lifeInfuserData.SetUIForInfuserStatus(true);

        // 첫 번째 활성 인퓨저 선택 (크기 및 투명도 설정)
        for (int i = 0; i + infuserNumber < InfuserManager.Instance.activatedInfusers.Length; i++)
        {
            if (InfuserManager.Instance.activatedInfusers[i + infuserNumber])
            {
                SelectReviveLife(i);
                break;
            }
        }

        // 카메라 설정
        infuserTrackedCamera.Follow = InfuserManager.Instance.infuser[infuserNumber].transform;
        infuserTrackedCamera.gameObject.SetActive(true);
    }

    public void SwitchActionMap(string actionMapName)
    {
        Debug.Log("Switching ActionMap to " + actionMapName);
        playerInput.SwitchCurrentActionMap(actionMapName);
    }

    public void GameOver()
    {
        SceneManager.LoadScene("StageSelection");
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Select/Left"].performed += OnLeftSelect;
            playerInput.actions["Select/Right"].performed += OnRightSelect;
            playerInput.actions["Select/Select"].performed += OnSelect;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Select/Left"].performed -= OnLeftSelect;
            playerInput.actions["Select/Right"].performed -= OnRightSelect;
            playerInput.actions["Select/Select"].performed -= OnSelect;
        }
    }

    public void OnLeftSelect(InputAction.CallbackContext context)
    {
        AudioManager.Instance.PlayPlayer("ui_click_fire_1", -1f);
        for (int i = 1; infuserNumber - i >= 0; i++)
        {
            if (InfuserManager.Instance.activatedInfusers[infuserNumber - i])
            {
                SelectReviveLife(-i);
                break;
            }
        }
    }

    public void OnRightSelect(InputAction.CallbackContext context)
    {
        AudioManager.Instance.PlayPlayer("ui_click_fire_1", 1f);
        for (int i = 1; i + infuserNumber < InfuserManager.Instance.activatedInfusers.Length; i++)
        {
            if (InfuserManager.Instance.activatedInfusers[i + infuserNumber])
            {
                SelectReviveLife(i);
                break;
            }
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        AudioManager.Instance.PlayPlayer("revival", 0f);

        // UI 축소 및 투명도 변경
        isBigger = false;
        AnimateSlotUI(infuserNumber, defaultSize, 0.5f);
        lifeInfuserData.SetUIForInfuserStatus(false);

        // 상태 업데이트
        InfuserManager.Instance.activatedInfusers[infuserNumber] = false;
        InfuserManager.Instance.canInfusion[infuserNumber] = true;

        Invoke(nameof(Revival), 1f);
    }

    private void Revival()
    {
        // 부활 후 UI 축소
        isBigger = false;
        AnimateSlotUI(infuserNumber, defaultSize, 0.5f);

        // 입력 및 카메라 복귀
        infuserTrackedCamera.gameObject.SetActive(false);
        SwitchActionMap("Player");

        var playerController = GetComponent<PlayerController>();
        playerController.SetKnockdownState(false);
        playerController.SetCanMove(true);
        obstacleManager.isRestoring = false;
        obstacleManager.ResetAllObstaclesSpeed();
    }

    public void SelectReviveLife(int amount)
    {
        // 이전 슬롯 작게
        AnimateSlotUI(infuserNumber, defaultSize, 0.5f);
        InfuserManager.Instance.infuserStatusChild[infuserNumber].transform.GetChild(0).gameObject.SetActive(false);

        int newIndex = infuserNumber + amount;

        // 새 슬롯 크게
        isBigger = true;
        AnimateSlotUI(newIndex, newSize, 1f);
        InfuserManager.Instance.infuserStatusChild[newIndex].transform.GetChild(0).gameObject.SetActive(true);

        // 이펙트 및 카메라 조정
        var selected = InfuserManager.Instance.infuser[newIndex];
        GameManager.Instance._shaderManager.LifeSacrificeEffect(
            selected.GetComponent<SpriteRenderer>().material,
            Camera.main.GetComponent<Volume>(),
            lifeInfuserData.infusionDuration);

        infuserTrackedCamera.Follow = selected.transform;
        infuserNumber = newIndex;
    }

    /// <summary>
    /// 인퓨저 UI 슬롯의 크기와 투명도를 Tween으로 변경
    /// </summary>
    /// <summary>
    /// 인퓨저 UI 슬롯의 크기와 투명도를 Tween으로 변경
    /// </summary>
    private void AnimateSlotUI(int index, Vector2 targetSize, float targetAlpha)
    {
        var slot = InfuserManager.Instance.infuserStatusChild[index];
        var rect = slot.GetComponent<RectTransform>();
        // UnityEngine.UI.Image를 명시적으로 사용하여 모호성 제거
        UnityEngine.UI.Image img = slot.GetComponent<UnityEngine.UI.Image>();

        Debug.Log($"[AnimateSlotUI] index:{index}, targetSize:{targetSize}, targetAlpha:{targetAlpha}, duration:{uiTweenDuration}, isBigger:{isBigger}");

        DOTween.Kill(rect);
        rect.DOSizeDelta(targetSize, uiTweenDuration).SetEase(Ease.InOutCubic); // 부드러운 In-Out Cubic 이징
        img.DOFade(targetAlpha, uiTweenDuration).SetEase(Ease.InOutCubic); // 부드러운 In-Out Cubic 이징
    }
}
