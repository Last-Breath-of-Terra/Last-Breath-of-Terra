using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 맵 정보를 관리하는 스크립트
/// </summary>

public enum MAP_TYPE
{
    Gravel,
    Sand,
    Wood,
    Default
}

public class MapManager
{
    private MAP_TYPE currentMapType;

    public MapManager()
    {
        UpdateCurrentMapType();
    }

    public void UpdateCurrentMapType()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("AlphaTest"))
        {
            currentMapType = MAP_TYPE.Gravel;
        }
        else if (sceneName.Contains("sand"))
        {
            currentMapType = MAP_TYPE.Sand;
        }
        else if (sceneName.Contains("wood"))
        {
            currentMapType = MAP_TYPE.Wood;
        }
        else
        {
            currentMapType = MAP_TYPE.Default;
        }
    }

    public MAP_TYPE GetCurrentMapType()
    {
        return currentMapType;
    }
}