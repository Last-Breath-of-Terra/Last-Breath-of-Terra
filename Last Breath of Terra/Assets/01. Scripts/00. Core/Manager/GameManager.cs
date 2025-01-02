using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager _ui;
    public ObstacleManager _obstacleManager;
    public ShaderManager _shaderManager;
    
    public Transform playerTr;

    private MapManager _map;

    public static MapManager Map { get { return Instance._map; } }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _map = new MapManager();
    }

    void Start()
    {
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        AudioManager.instance.PlayAmbience("map_1_stage_ambience");

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

}
