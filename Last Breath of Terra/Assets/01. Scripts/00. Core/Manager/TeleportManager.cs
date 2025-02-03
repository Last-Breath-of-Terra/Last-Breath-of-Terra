using System.Collections;
using Cinemachine;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using DG.Tweening;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
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
        player.GetComponent<Rigidbody2D>().gravityScale = 0;

        //※※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.instance.FadeOutBGM(1f);
        AudioManager.instance.FadeOutAmbience(1f);

        int teleportOffset = 2;
        DOTween.To(() => player.transform.position, x => player.transform.position = x,
            player.transform.position + teleportDirection * 2, 1f);
        float f = 0f;
        while (f <= 1f)
        {
            f += 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }

        animator.SetBool("MoveToPortal", false);
        //animator.Play("Idle");
        if (teleportDirection == new Vector3(0, 1, 0))
        {
            //player.transform.position = teleportSet[targetID].GetComponent<Teleport>().targetPos.position;
            teleportOffset = 3;
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
            //DOTween.To(() => player.transform.position, x => player.transform.position = x, player.transform.position + new Vector3(2, 0, 0), 2.5f);

            Vector3 startPosition = player.transform.position;
            Vector3 targetPosition;
            
            if (teleportSet[targetID].GetComponent<Teleport>().isRight)
            {
                targetPosition = startPosition + new Vector3(2, 0, 0);
            }
            else
            {
                targetPosition = startPosition - new Vector3(2, 0, 0);
            }

            // 중간 높이 포인트 설정 (포물선의 정점)
            Vector3 controlPoint = startPosition + new Vector3(1, 2, 0);

            Vector3[] path = new Vector3[] { startPosition, controlPoint, targetPosition };

            player.transform.DOPath(path, 3f, PathType.CatmullRom).SetEase(Ease.InOutQuad);
        }
        else if (teleportDirection == new Vector3(0, -1, 0))
        {
            animator.Play("Idle");
            player.transform.position =
                teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
        }
        else
        {
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
        }

        // ※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.instance.UpdatePlayerAuidoSettingsByMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
        if (AudioManager.instance.mapAmbienceDict.ContainsKey(teleportSet[targetID].GetComponent<Teleport>().mapID))
        {
            AudioManager.instance.PlayAmbienceForSceneAndMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
            AudioManager.instance.FadeInAmbience(1f);
        }
        else
        {
            AudioManager.instance.PlayAmbienceForSceneAndMap(teleportSet[targetID].GetComponent<Teleport>().mapID);
            AudioManager.instance.FadeInBGM(1f);
            AudioManager.instance.FadeInAmbience(1f);
        }

        // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        if (parallaxBackgroundObject != null)
        {
            ParallaxBackground parallaxBackground = parallaxBackgroundObject.GetComponent<ParallaxBackground>();
            if (parallaxBackground != null)
            {
                // teleportSet[targetID]의 mapID 값을 가져옴
                Teleport teleport = teleportSet[targetID].GetComponent<Teleport>();
                if (teleport != null)
                {
                    parallaxBackground.mapID = teleport.mapID;
                }
                else
                {
                    Debug.LogWarning("Teleport script not found on teleportSet[targetID].");
                }

                // 배경 위치를 플레이어 위치와 같게 설정
                parallaxBackgroundObject.transform.position = new Vector3(
                    player.transform.position.x,
                    player.transform.position.y,
                    parallaxBackgroundObject.transform.position.z
                );
            }
            else
            {
                Debug.LogWarning("ParallaxBackground script not found on the referenced object.");
            }
        }
        else
        {
            Debug.LogWarning("ParallaxBackground object is not assigned in the Inspector.");
        }
        // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※


        player.GetComponent<Rigidbody2D>().gravityScale = 3;
        ChangeCamera(teleportSet[targetID].GetComponent<Teleport>().mapID);
        yield return new WaitForSeconds(0.5f);
        while (f > 0f)
        {
            f -= 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        player.GetComponent<PlayerController>().canMove = true;
    }

    public void MoveToPortal()
    {
        player.GetComponent<PlayerController>().canMove = false;
        Debug.Log("MoveToPortal called");
        animator.SetBool("MoveToPortal", true);
    }
}