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

public class LifeRestorer : MonoBehaviour
{
    public PlayerSO playerData;
    public StageLifeInfuserSO lifeInfuserData;
    public CinemachineVirtualCamera camera;
    public CinemachineVirtualCamera infuserTrackedCamera;
    public ObstacleManager obstacleManager;
    
    [SerializeField] private InputActionMap selectMap;

    private PlayerInput playerInput;

    //private int currentIndex = 0;
    private Vector2 defaultSize = new Vector2(174, 271);
    private Vector2 newSize;
    private int infuserNumber;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        lifeInfuserData.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        lifeInfuserData.SetUITransparency(lifeInfuserData.InfuserStatusUI.transform, -0.9f);
        //lifeInfuserData.SetUIForInfuserStatus(false);

        //defaultSize = lifeInfuserData.infuserStatusUI[0].GetComponent<RectTransform>().sizeDelta;
        newSize = defaultSize * 1.2f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hp : " + playerData.hp);
        if (collision.transform.CompareTag("Obstacle"))
        {
            //부활에 사용할 생명력?이 남아있지 않다면
            if (lifeInfuserData.infusedLifeCount <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (playerData.hp <= 0)
            {
                //장애물 이동 멈추기
                obstacleManager.StopAllObstacles();
                //마우스 조작 변경
                SwitchActionMap("Select");
                //왼쪽 끝에서부터 설정
                infuserNumber = 0;
                //UpdateRectSize(infuserNumber, newSize);
                //UI 활성화
                DOTween.To(() => lifeInfuserData.InfuserStatusUI.GetComponent<RectTransform>().localScale, x => lifeInfuserData.InfuserStatusUI.GetComponent<RectTransform>().localScale = x, new Vector3(1f, 1f, 1f), 0.1f);
                //lifeInfuserData.SetUIForInfuserStatus(true);
                //lifeInfuserData.SetUITransparency(lifeInfuserData.InfuserStatusUI.transform, 1.0f);
                //lifeInfuserData.infuserStatusUI[infuserNumber].transform.GetChild(0).gameObject.SetActive(true);
                for (int i = 0; i + infuserNumber< lifeInfuserData.canInfusion.Length; i++)
                {
                    if (!lifeInfuserData.canInfusion[i + infuserNumber])
                    {
                        if (i + infuserNumber <= lifeInfuserData.canInfusion.Length)
                        {
                            SelectReviveLife(i);

                        }
                        break;
                    }
                }

                //카메라 이동
                infuserTrackedCamera.Follow = lifeInfuserData.infuser[infuserNumber].transform;
                //infuserTrackedCamera.transform.position = lifeInfuserData.infuser[infuserNumber].transform.position;
                infuserTrackedCamera.gameObject.SetActive(true);
            }
        }
    }

    public void SwitchActionMap(string actionMapName)
    {
        Debug.Log("switchActionMap");
        playerInput.SwitchCurrentActionMap(actionMapName);
    }


    public void GameOver()
    {
        SceneManager.LoadScene("StageSelection");
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
        AudioManager.instance.PlayPlayer("ui_click_fire_1", -1f);
        for (int i = 1; infuserNumber - i >= 0; i++)
        {
            if (!lifeInfuserData.canInfusion[infuserNumber - i])
            {
                Debug.Log("leftSelect");
                SelectReviveLife(-i);
                break;
            }
        }
    }

    public void OnRightSelect(InputAction.CallbackContext context)
    {
        AudioManager.instance.PlayPlayer("ui_click_fire_1", 1f);
        for (int i = 1; i + infuserNumber < lifeInfuserData.canInfusion.Length; i++)
        {
            if (!lifeInfuserData.canInfusion[i + infuserNumber])
            {
                Debug.Log("rightSelect");
                SelectReviveLife(i);
                break;
            }
        }
        //SelectReviveLife(infuserNumber + 1);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        //부활사운드
        AudioManager.instance.PlayPlayer("revival", 0f);
        //비활성화
        lifeInfuserData.infuser[infuserNumber].GetComponent<SpriteRenderer>().sprite = lifeInfuserData.InfuserInactiveImage;
        lifeInfuserData.infuserStatusUI[infuserNumber].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
        lifeInfuserData.isInfuser[infuserNumber] = false;
        lifeInfuserData.canInfusion[infuserNumber] = true;

        //UI 비활성화
        //lifeInfuserData.SetUIForInfuserStatus(false);
        //lifeInfuserData.SetUITransparency(lifeInfuserData.InfuserStatusUI.transform, 0.2f);
        lifeInfuserData.infuserStatusUI[infuserNumber].transform.GetChild(0).gameObject.SetActive(false);
        lifeInfuserData.infuserStatusUI[infuserNumber].GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
        DOTween.To(() => lifeInfuserData.InfuserStatusUI.GetComponent<RectTransform>().localScale, x => lifeInfuserData.InfuserStatusUI.GetComponent<RectTransform>().localScale = x, new Vector3(0.5f, 0.5f, 0.5f), 0.1f);


        UpdateRectSize(infuserNumber, defaultSize);

        //카메라 비활성화
        infuserTrackedCamera.gameObject.SetActive(false);

        //마우스 조작 변경
        SwitchActionMap("Player");
        /*
        if (lifeInfuserData.isInfuser[infuserNumber])
        {
            나중에 추가..
        }*/
        //장애물 이동 활성화
        obstacleManager.ResetAllObstaclesSpeed();
    }

    public void SelectReviveLife(int amount)
    {
        //기존 값
        UpdateRectSize(infuserNumber, defaultSize);
        lifeInfuserData.infuserStatusUI[infuserNumber].transform.GetChild(0).gameObject.SetActive(false);

        //새로운 값
        Debug.Log("infuserNumber : " + infuserNumber + ", amount : " + amount + " newInfuserNumber : " + infuserNumber + amount);
        infuserNumber += amount;//Mathf.Clamp(setValue, 0, lifeInfuserData.totalInfuser);
        lifeInfuserData.infuserStatusUI[infuserNumber].transform.GetChild(0).gameObject.SetActive(true);
        UpdateRectSize(infuserNumber, newSize);

        //카메라 이동
        infuserTrackedCamera.Follow = lifeInfuserData.infuser[infuserNumber].transform;
    }

    private void UpdateRectSize(int setValue, Vector2 changeSize)
    {
        RectTransform rectTransform = lifeInfuserData.infuserStatusUI[setValue].GetComponent<RectTransform>();
        rectTransform.sizeDelta = changeSize;
    }
}