using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public int stage;
    
    public void ChangeStage(string stage)
    {
        if(stage == "Tutorial")
            SceneManager.LoadScene("Tutorial");
        else
        {
            StoryManager.Instance.ActivateStoryForScene(stage);
            SceneManager.LoadScene("StoryScene");
        }
        // SceneManager.LoadScene(stage);
    }
}
