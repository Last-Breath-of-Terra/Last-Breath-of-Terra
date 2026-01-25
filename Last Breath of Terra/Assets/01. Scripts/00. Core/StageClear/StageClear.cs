using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using DG.Tweening;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class StageClear : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Image fadeImage;
    public Sprite[] changeObjectImage;
    public GameObject iceWall;                 // stage2에서만 필요
    public CinemachineVirtualCamera StageClearCamera;

    [Header("Camera Move")]
    public float targetYOffset = 5f;
    public float duration = 2f;

    [Header("Stage")]
    public bool isStage1;

    private bool isClearing = false;

    private void Start()
    {
        StartCoroutine(CoClearAndGo());
    }

    IEnumerator Fade(bool isCameraOn)
    {
        if (fadeImage == null) yield break;

        for (float i = 0; i < 1f; i += 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        if (StageClearCamera != null)
            StageClearCamera.gameObject.SetActive(isCameraOn);

        for (float i = 1; i > 0f; i -= 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }

    IEnumerator MoveCamera()
    {
        yield return StartCoroutine(Fade(true));

        if (StageClearCamera == null) yield break;

        var transposer = StageClearCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (transposer == null) yield break;

        Vector3 currentOffset = transposer.m_TrackedObjectOffset;

        DOTween.To(
            () => currentOffset.y,
            y =>
            {
                currentOffset.y = y;
                transposer.m_TrackedObjectOffset = currentOffset;
            },
            targetYOffset,
            duration
        ).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(Fade(false));
    }

    IEnumerator BreakIceWall()
    {
        if (iceWall == null)
        {
            Debug.LogError("StageClear: iceWall이 Inspector에 할당되지 않았습니다!");
            yield break;
        }

        yield return StartCoroutine(Fade(true));

        var sr = iceWall.GetComponent<SpriteRenderer>();
        if (sr != null && changeObjectImage != null && changeObjectImage.Length >= 2)
        {
            sr.sprite = changeObjectImage[0];
            yield return new WaitForSeconds(2f);
            sr.sprite = changeObjectImage[1];
            yield return new WaitForSeconds(2f);
        }

        var col = iceWall.GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = false;

        yield return StartCoroutine(Fade(false));
    }

    private IEnumerator CoClearAndGo()
    {
        Debug.Log("stage cleared");

        if (isStage1)
            yield return StartCoroutine(MoveCamera());
        else
            yield return StartCoroutine(BreakIceWall());
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isClearing) return;
        if (!collision.CompareTag("Player")) return;

        int count = InfuserManager.Instance.activatedInfusers.Count(x => x);
        Debug.Log(count + " infusers activated");

        if (count >= 10)
        {
            isClearing = true; 
            DataManager.Instance.ModifyPlayerData(DataManager.Instance.playerIndex, 0, true);
            StoryManager.Instance.ActivateStoryForScene("Stage1ExitStory");
            SceneManager.LoadScene("StoryScene");
            //SceneManager.LoadScene("StageSelection");
        }
    }
}
