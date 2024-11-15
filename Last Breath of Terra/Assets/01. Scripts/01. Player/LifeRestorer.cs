using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;
using Unity.VisualScripting;

public class LifeRestorer : MonoBehaviour
{
    public PlayerSO playerData;
    public StageLifeInfuserSO lifeInfuserData;
    public CinemachineVirtualCamera camera;
    public CinemachineVirtualCamera infuserTrackedCamera;

    [SerializeField] private InputActionMap selectMap;
    private PlayerInput playerInput;
    private int currentIndex = 0;
    private Vector2 defaultSize = new Vector2(174, 271);
    private Vector2 newSize;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        lifeInfuserData.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        
        //defaultSize = lifeInfuserData.infuserStatusUI[0].GetComponent<RectTransform>().sizeDelta;
        newSize = defaultSize * 1.5f;
    }
    /*
    public void LifeRestorer()
    {
        playerInput.SwitchCurrentActionMap("Select");
        
    }
    */

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
        //기존 값
        UpdateRectSize(currentIndex, defaultSize);
        //새로운 값
        currentIndex = Mathf.Clamp(currentIndex - 1, 0, lifeInfuserData.totalInfuser);
        UpdateRectSize(currentIndex, newSize);
        //lifeInfuserData.infuserStatusUI[currentIndex].color = Color.cyan;
        //camera.Follow = lifeInfuserData.infuser[currentIndex].transform;
        camera.Follow = null;
        camera.transform.position = lifeInfuserData.infuser[currentIndex].transform.position;
        camera.Follow = lifeInfuserData.infuser[currentIndex].transform;

        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
        
    }

    public void OnRightSelect(InputAction.CallbackContext context)
    {
        UpdateRectSize(currentIndex, defaultSize);
        //lifeInfuserData.infuserStatusUI[currentIndex].color = Color.gray;
        currentIndex = Mathf.Clamp(currentIndex + 1, 0, lifeInfuserData.totalInfuser);
        UpdateRectSize(currentIndex, newSize);
        //lifeInfuserData.infuserStatusUI[currentIndex].color = Color.cyan;
        camera.Follow = null;
        camera.transform.position = lifeInfuserData.infuser[currentIndex].transform.position;
        if (camera.transform.position == lifeInfuserData.infuser[currentIndex].transform.position)
        {
            camera.Follow = lifeInfuserData.infuser[currentIndex].transform;
        }

        //camera.Follow = lifeInfuserData.infuser[currentIndex].transform;

        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        camera.Follow = transform;
    }

    public void SelectReviveLife()
    {
        currentIndex = 0;
        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
    }

    private void UpdateRectSize(int currentIndex, Vector2 changeSize)
    {
        RectTransform rectTransform = lifeInfuserData.infuserStatusUI[currentIndex].GetComponent<RectTransform>();
        rectTransform.sizeDelta = changeSize;
    }
}
