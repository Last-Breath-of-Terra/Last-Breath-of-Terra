using UnityEngine.SceneManagement;

/// <summary>
/// 씬 정보를 관리하는 스크립트
/// </summary>

public enum SCENE_TYPE
{
    Title,
    Tutorial,
    Gravel,
    Stage2,
    Default
}

public class ScenesManager : Singleton<ScenesManager>
{
    private SCENE_TYPE currentSceneType;

    public ScenesManager()
    {
        UpdateCurrentSceneType();
    }

    public void UpdateCurrentSceneType()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Title"))
        {
            currentSceneType = SCENE_TYPE.Title;
        }
        else if (sceneName.Contains("Tutorial"))
        {
            currentSceneType = SCENE_TYPE.Tutorial;
        }
        else if (sceneName.Contains("gravel"))
        {
            currentSceneType = SCENE_TYPE.Gravel;
        }
        else if (sceneName.Contains("Stage2"))
        {
            currentSceneType = SCENE_TYPE.Stage2;
        }
        else
        {
            currentSceneType = SCENE_TYPE.Default;
        }
    }

    public SCENE_TYPE GetCurrentSceneType()
    {
        return currentSceneType;
    }
}