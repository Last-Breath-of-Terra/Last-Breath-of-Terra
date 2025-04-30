using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    public UIManager _ui;
    public ObstacleManager _obstacleManager;
    public ShaderManager _shaderManager;
    public StageMinimapManager _stageminimapManager;
    
    public Transform playerTr;

    private ScenesManager _scenesManager;

    public static ScenesManager ScenesManager { get { return Instance._scenesManager; } }


    void Awake()
    {

        _scenesManager = new ScenesManager();
    }

    void Start()
    {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        AudioManager.Instance.PlayAmbience("map_1_stage_ambience");

        UpdateManagersReference();
        UpdatePlayerReference();
    }
    

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateManagersReference();
        UpdatePlayerReference();
        
        switch (scene.name)
        {
            case "Tutorial":
                Cursor.visible = false;
                break;
            case "Stage1_gravel":
                Cursor.visible = false;
                break;
            default:
                Cursor.visible = true;
                break;
        }
    
    }

    private void UpdateManagersReference()
    {
        if (_ui == null)
        {
            _ui = GameObject.FindObjectOfType<UIManager>();
        }
        if (_obstacleManager == null)
        {
            _obstacleManager = GameObject.FindObjectOfType<ObstacleManager>();
        }
        if (_shaderManager == null)
        {
            _shaderManager = GameObject.FindObjectOfType<ShaderManager>();
        }
    }

    private void UpdatePlayerReference()
    {
        PlayerController playerController = GameObject.FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerTr = playerController.transform;
        }
        else
        {
            Debug.LogWarning("Player not found in the current scene.");
        }
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
