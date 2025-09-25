using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LifeRestorer : MonoBehaviour
{
    public LifeInfuserSO lifeInfuserData;
    public CinemachineVirtualCamera camera;
    public CinemachineVirtualCamera infuserTrackedCamera;
    public ObstacleManager obstacleManager;
    
    [SerializeField] private InputActionMap selectMap;

    private PlayerInput playerInput;
    private Vector2 defaultSize; //= new Vector2(174, 271);
    private Vector2 newSize;
    private int infuserNumber;
    private float uiTweenDuration = 2f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        InfuserManager.Instance.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        Transform[] transform = InfuserManager.Instance.infuserStatusChild;
        defaultSize = InfuserManager.Instance.infuserStatusChild[0].GetComponent<RectTransform>().sizeDelta;
        newSize = defaultSize * 1.2f;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Obstacle"))
        {
            var playerController = gameObject.GetComponent<PlayerController>();

            if (!InfuserManager.Instance.activatedInfusers.Any(infuser => infuser))
            {
                SceneManager.LoadScene("StageSelection");
            }
            else if (playerController.HP <= 0)
            {
                playerController.SetKnockdownState(true);
                obstacleManager.isRestoring = true;
                obstacleManager.StopAllObstacles();
                Invoke("StartLifeRestorer", 1f);
            }
        }
    }

    private void StartLifeRestorer()
    {
        Debug.Log("Deactivating obstacle");
        
        //마우스 조작 변경
        SwitchActionMap("Select");
        //왼쪽 끝에서부터 설정
        infuserNumber = 0;
        //UpdateRectSize(infuserNumber, newSize);
        //UI 활성화
        DOTween.To(() => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale, x => InfuserManager.Instance.infuserStatus.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), 0.1f);
        lifeInfuserData.SetUIForInfuserStatus(true);
        //lifeInfuserData.SetUITransparency(lifeInfuserData.InfuserStatusUI.transform, 1.0f);
        //lifeInfuserData.infuserStatusUI[infuserNumber].transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i + infuserNumber < InfuserManager.Instance.activatedInfusers.Length; i++)
        {
            if (InfuserManager.Instance.activatedInfusers[i + infuserNumber])
            {
                if (i + infuserNumber <= InfuserManager.Instance.activatedInfusers.Length)
                {
                    SelectReviveLife(i);

                }
                break;
            }
        }

        //카메라 이동
        infuserTrackedCamera.Follow = InfuserManager.Instance.infuser[infuserNumber].transform;
        //infuserTrackedCamera.transform.position = lifeInfuserData.infuser[infuserNumber].transform.position;
        infuserTrackedCamera.gameObject.SetActive(true);
    }

    public void SwitchActionMap(string actionMapName)
    {
        playerInput.SwitchCurrentActionMap(actionMapName);
    }
    
    #region SelectInputSetting

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

    #endregion


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
        //부활사운드
        AudioManager.Instance.PlayPlayer("revival", 0f);
        
        //오브젝트 변경
        InfuserManager.Instance.infuser[infuserNumber].GetComponent<SpriteRenderer>().sprite = lifeInfuserData.InfuserInactiveImage[InfuserManager.Instance.infuser[infuserNumber].GetComponent<LifeInfuser>().infuserType];
        InfuserManager.Instance.infuser[infuserNumber].GetComponent<SpriteRenderer>().material = lifeInfuserData.defaultMaterial;
        //자식 오브젝트 변경
        InfuserManager.Instance.infuserStatusChild[infuserNumber].transform.GetChild(0).gameObject.SetActive(false);
        InfuserManager.Instance.infuserStatusChild[infuserNumber].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.4f);

        //상태 업데이트
        InfuserManager.Instance.activatedInfusers[infuserNumber] = false;
        InfuserManager.Instance.canInfusion[infuserNumber] = true;
        Invoke(nameof(Revival), 1f);
    }

    private void Revival()
    {
        //UI 변경
        UpdateRectSize(infuserNumber, defaultSize);
        lifeInfuserData.SetUIForInfuserStatus(false);

        //카메라 비활성화
        infuserTrackedCamera.gameObject.SetActive(false);

        //마우스 조작 변경
        SwitchActionMap("Player");

        var playerController = gameObject.GetComponent<PlayerController>();
        playerController.SetKnockdownState(false);
        playerController.SetCanMove(true);
        obstacleManager.isRestoring = false;
        obstacleManager.ResetAllObstaclesSpeed();
    }

    public void SelectReviveLife(int amount)
    {
        //선택한 오브젝트 빛나기...
        GameObject selectedInfuser = InfuserManager.Instance.infuser[infuserNumber];
        selectedInfuser.GetComponent<SpriteRenderer>().material.SetFloat("_Enabled", 1f);
        InfuserManager.Instance.infuser[infuserNumber+1].GetComponent<SpriteRenderer>().material.SetFloat("_Enabled", 0f);
        GameManager.Instance._shaderManager.LifeSacrificeEffect(
            selectedInfuser.GetComponent<SpriteRenderer>().material, 
            Camera.main.GetComponent<Volume>(), 
            lifeInfuserData.infusionDuration);

        //기존 값
        UpdateRectSize(infuserNumber, defaultSize);
        InfuserManager.Instance.infuserStatusChild[infuserNumber].transform.GetChild(0).gameObject.SetActive(false);

        //새로운 값
        Debug.Log("infuserNumber : " + infuserNumber + ", amount : " + amount + " newInfuserNumber : " + infuserNumber + amount);
        infuserNumber += amount;//Mathf.Clamp(setValue, 0, lifeInfuserData.totalInfuser);
        InfuserManager.Instance.infuserStatusChild[infuserNumber].transform.GetChild(0).gameObject.SetActive(true);
        UpdateRectSize(infuserNumber, newSize);


        //카메라 이동
        infuserTrackedCamera.Follow = InfuserManager.Instance.infuser[infuserNumber].transform;
    }
    
    private void UpdateRectSize(int setValue, Vector2 changeSize)
    {
        RectTransform rectTransform = InfuserManager.Instance.infuserStatusChild[setValue].GetComponent<RectTransform>();
        rectTransform.sizeDelta = changeSize;
        rectTransform.DOSizeDelta(changeSize, uiTweenDuration).SetEase(Ease.InOutCubic); 
        
    }
}