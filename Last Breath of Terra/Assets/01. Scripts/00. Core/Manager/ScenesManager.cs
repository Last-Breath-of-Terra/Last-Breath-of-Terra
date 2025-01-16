using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 정보를 관리하는 스크립트
/// </summary>

public enum SCENE_TYPE
{
    Tutorial,
    Gravel,
    Sand,
    Wood,
    Default
}

public class ScenesManager
{
    private SCENE_TYPE currentMapType;

    public ScenesManager()
    {
        UpdateCurrentSceneType();
    }

    public void UpdateCurrentSceneType()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Tutorial"))
        {
            currentMapType = SCENE_TYPE.Tutorial;
        }
        else if (sceneName.Contains("gravel"))
        {
            currentMapType = SCENE_TYPE.Gravel;
        }
        else if (sceneName.Contains("sand"))
        {
            currentMapType = SCENE_TYPE.Sand;
        }
        else if (sceneName.Contains("wood"))
        {
            currentMapType = SCENE_TYPE.Wood;
        }
        else
        {
            currentMapType = SCENE_TYPE.Gravel;

            //currentMapType = MAP_TYPE.Default;
        }
    }

    public SCENE_TYPE GetCurrentMapType()
    {
        return currentMapType;
    }
}