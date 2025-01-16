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
    Intro,
    Tutorial,
    Gravel,
    Sand,
    Wood,
    Default
}

public class ScenesManager
{
    private SCENE_TYPE currentSceneType;

    public ScenesManager()
    {
        UpdateCurrentSceneType();
    }

    public void UpdateCurrentSceneType()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Intro"))
        {
            currentSceneType = SCENE_TYPE.Intro;
        }
        else if (sceneName.Contains("Tutorial"))
        {
            currentSceneType = SCENE_TYPE.Tutorial;
        }
        else if (sceneName.Contains("gravel"))
        {
            currentSceneType = SCENE_TYPE.Gravel;
        }
        else if (sceneName.Contains("sand"))
        {
            currentSceneType = SCENE_TYPE.Sand;
        }
        else if (sceneName.Contains("wood"))
        {
            currentSceneType = SCENE_TYPE.Wood;
        }
        else
        {
            currentSceneType = SCENE_TYPE.Gravel;

            //currentMapType = MAP_TYPE.Default;
        }
    }

    public SCENE_TYPE GetCurrentSceneType()
    {
        return currentSceneType;
    }
}