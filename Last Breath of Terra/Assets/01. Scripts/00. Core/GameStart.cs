using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameStart : MonoBehaviour
{
    public void OnButtonClick()
    {
        string playerName = gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        DataManager.Instance.playerIndex = DataManager.Instance.FindPlayerIndexByName(playerName);
        
        StoryManager.Instance.ActivateStoryForScene("TitleStory");

        SceneManager.LoadScene("StoryScene");
    }
    
}
