using UnityEngine;

public class StorySelectManager : MonoBehaviour
{
    private void Start()
    {
        GameObject.Find("Story").transform.Find(StoryManager.Instance.storyName).gameObject.SetActive(true);
    }
}
