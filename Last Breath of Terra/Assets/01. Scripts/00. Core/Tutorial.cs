using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class Tutorial : MonoBehaviour
{
    public GameObject door;
    public GameObject obstacle;
    public GameObject revivalPractice;
    private void Update()
    {
        int count = InfuserManager.Instance.activatedInfusers.Count(x => x);
        if (count >= 5) //추후 변경 필요
        {
            door.SetActive(false);
            if (revivalPractice != null)
            {
                revivalPractice.SetActive(true);
            }
        }
        else
        {
            door.SetActive(true);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "RevivalPractice")
        {
            gameObject.GetComponent<PlayerController>().canMove = false;
            obstacle.SetActive(true);
            Destroy(other.gameObject);
        }

        if (other.gameObject.name == "TutorialClear")
        {
            SceneManager.LoadScene("StageSelection");
//            StoryManager.Instance.ActivateStoryForScene("TutorialStory");
  //          SceneManager.LoadScene("StoryScene");
        }
        
    }
}
