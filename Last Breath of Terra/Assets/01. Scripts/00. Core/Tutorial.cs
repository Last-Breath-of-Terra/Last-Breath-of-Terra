using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;


public class Tutorial : MonoBehaviour
{
    public GameObject door;
    public GameObject obstacle;
    public GameObject revivalPractice;
    public Image fadePanel;

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
            StartCoroutine(LoadGameStage());
        }
    }

    private IEnumerator LoadGameStage()
    {
        fadePanel.gameObject.SetActive(true);

        fadePanel.color = new Color(1f, 1f, 1f, 0f);
        yield return fadePanel.DOFade(1f, 1f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return fadePanel.DOColor(Color.black, 1f).SetEase(Ease.InQuad).WaitForCompletion();
        StoryManager.Instance.ActivateStoryForScene("TutorialStory");
        SceneManager.LoadScene("StoryScene");
    }
}
