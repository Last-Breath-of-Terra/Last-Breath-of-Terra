using System.Collections;
using Cinemachine;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using DG.Tweening;

public class TeleportManager : Singleton<TeleportManager>
{
    public GameObject[] teleportSet;
    public PolygonCollider2D[] camBorders;
    public CinemachineVirtualCamera virtualCamera;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private GameObject player;
    private Animator animator;

    // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
    public GameObject parallaxBackgroundObject;
    // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※

    private void Start()
    {
        Debug.Log("teleportSet.Length : " + teleportSet.Length);
        player = GameObject.FindGameObjectWithTag("Player");
        animator = player.GetComponent<Animator>();
    }

    public void ChangeCamera(int mapID)
    {
        var confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            // CinemachineConfiner가 없다면 추가하기
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
            Debug.Log("CinemachineConfiner component added.");
        }

        Debug.Log("mapID" + mapID);
        virtualCamera.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = camBorders[mapID];
    }

    public void CoFade(int targetID, Vector3 teleportDirection)
    {
        StartCoroutine(Fade(targetID, teleportDirection));
    }

    IEnumerator Fade(int targetID, Vector3 teleportDirection)
    {
        //※※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.Instance.FadeOutBGM(1f);
        AudioManager.Instance.FadeOutAmbience(1f);

        int teleportOffset = 2;

        //포탈 들어가기 전
        if (teleportDirection == new Vector3(0, 1, 0)) //위로 갈 때
        {
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
            animator.SetBool("isJumping", true);
        }
        else if (teleportDirection == new Vector3(0, -1, 0))
        {
            animator.SetBool("isFalling", true);
        }

        if (teleportDirection == new Vector3(1, 0, 0) || teleportDirection == new Vector3(-1, 0, 0))
        {
            animator.SetBool("MoveToPortal", true);
            player.transform.DOMove(player.transform.position + teleportDirection * 2, fadeDuration);
            //DOTween.To(() => player.transform.position, x => player.transform.position = x, player.transform.position + teleportDirection * 2, 1f);
        }

        //fadein
        for (float i = 0; i < 1f; i += 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        //animator.Play("Idle");
//        virtualCamera.enabled = false;
        //포탈이동
        /*
        virtualCamera.Follow = null;
        virtualCamera.transform.position = teleportSet[targetID].transform.position;
                virtualCamera.Follow = player.transform;

        */
        player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;

        ChangeCamera(teleportSet[targetID].GetComponent<Teleport>().mapID);
        
        
        yield return new WaitForSeconds(1f);



        //포탈 나와서
        if (teleportDirection == new Vector3(0, 1, 0)) //올라옴
        {
            //player.transform.position = teleportSet[targetID].GetComponent<Teleport>().targetPos.position;
            teleportOffset = 3;
            //player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
            //DOTween.To(() => player.transform.position, x => player.transform.position = x, player.transform.position + new Vector3(2, 0, 0), 2.5f);

            Vector3 targetPosition;

            if (teleportSet[targetID].GetComponent<Teleport>().isRight)
            {
                targetPosition = player.transform.position + new Vector3(3, 2, 0);
            }
            else
            {
                targetPosition = player.transform.position + new Vector3(-3, 2, 0);
            }

            // 중간 높이 포인트 설정 (포물선의 정점)

            player.transform.DOJump(targetPosition, 3, 1, 1);
            //layer.transform.DOPath(path, 3f, PathType.CatmullRom);//.SetEase(Ease.InOutQuad);
        } /*
        else if (teleportDirection == new Vector3(0, -1, 0)) //내려옴
        {

            //player.transform.DOMove(player.transform.position + teleportOffset * teleportDirection, fadeDuration);

            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
        }*/
        else
        {
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
            yield return new WaitForSeconds(0.1f);

            //player.transform.DOMove(player.transform.position + teleportDirection * 2, fadeDuration);

            //player.transform.DOMove(player.transform.position + teleportOffset * teleportDirection, fadeDuration);
        }

        #region audio

        // ※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.Instance.UpdatePlayerAuidoSettingsByMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
        if (AudioManager.Instance.mapAmbienceDict.ContainsKey(teleportSet[targetID].GetComponent<Teleport>().mapID))
        {
            AudioManager.Instance.PlayAmbienceForSceneAndMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
            AudioManager.Instance.FadeInAmbience(1f);
        }
        else
        {
            AudioManager.Instance.PlayAmbienceForSceneAndMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
            AudioManager.Instance.FadeInBGM(1f);
            AudioManager.Instance.FadeInAmbience(1f);
        }

        #endregion

        if (teleportDirection == new Vector3(0, 1, 0))
        {
            player.GetComponent<Rigidbody2D>().gravityScale = 3;
            animator.SetBool("Jump", false);
        }
        else if (teleportDirection == new Vector3(0, -1, 0))
        {
            animator.SetBool("isFalling", false);
        }
        else
        {
            animator.SetBool("MoveToPortal", false);
        }

        GameManager.Instance._stageminimapManager.OnMapEntered("MAP" + teleportSet[targetID].GetComponent<Teleport>()
            .mapID);

        //yield return new WaitForSeconds(0.5f);

        //fadeout
        for (float i = 1; i > 0f; i -= 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);


        //플레이어 이동 
        player.GetComponent<PlayerController>().canMove = true;
    }

    public void MoveToPortal()
    {
        player.GetComponent<PlayerController>().canMove = false;
        Debug.Log("MoveToPortal called");
        //animator.SetBool("MoveToPortal", true);
    }
}