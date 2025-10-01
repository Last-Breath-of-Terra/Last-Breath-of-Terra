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

    [SerializeField] LayerMask mask;

    private GameObject player;
    private Animator animator;

    // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
    public GameObject parallaxBackgroundObject;
    // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※

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
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
        }

        virtualCamera.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = camBorders[mapID];
    }


    //텔레포트 시작 코루틴 시작
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
        if (teleportDirection == Vector3.up) //위로 갈 때
        {
            player.GetComponent<Rigidbody2D>().gravityScale = 0;
            animator.SetBool("isJumping", true);
        }
        else if (teleportDirection != Vector3.down)
        {
            animator.SetBool("MoveToPortal", true);
            player.transform.DOMove(player.transform.position + teleportDirection * 2, fadeDuration);
        }

        gameObject.GetComponent<GimmickChange>().ChangeGimmick(teleportSet[targetID].GetComponent<Teleport>().mapID);


        //fadein 
        for (float i = 0; i < 1f; i += 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        player.transform.DOKill();
        player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
        ChangeCamera(teleportSet[targetID].GetComponent<Teleport>().mapID);
        //GimmickShooterManager.Instance.ChangeGimmickGroup(teleportSet[targetID].GetComponent<Teleport>().mapID);

        yield return new WaitForSeconds(0.2f);

        

        //포탈 나와서
        if (teleportDirection == Vector3.up)
        {
            Vector3 direction;
            float dist = 1f;
            if (teleportSet[targetID].GetComponent<Teleport>().isRight) direction = Vector2.right;
            else direction = Vector2.left;
            direction *= 3;
            bool checkDestance = true;
            RaycastHit2D hit = Physics2D.Raycast(player.transform.position, direction, 1f, mask);
            while (true) 
            {
                hit = Physics2D.Raycast(player.transform.position, direction, 1f, mask);
                Debug.DrawRay(player.transform.position, direction, Color.red, 1f);
                if (hit.collider == null)
                    break;
                Debug.Log("올라가는중" + hit.collider.gameObject.name);
                if (checkDestance)
                {
                    if (hit.collider != null)
                    {
                        dist = hit.distance;
                    }
                    checkDestance = false;
                }
                player.transform.position += Vector3.up;
                yield return new WaitForSeconds(0.01f);
            }

            Vector3 targetPosition = player.transform.position + dist * direction;

            // 중간 높이 포인트 설정 (포물선의 정점)

            player.transform.DOJump(targetPosition, 2, 1, 0.5f);
        }
        else if (teleportDirection != Vector3.down)
        {
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
            yield return new WaitForSeconds(0.1f);
        }

        #region audio

        // ※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.Instance.SwitchAmbienceAndBGM(teleportSet[targetID].GetComponent<Teleport>().mapID);

        #endregion

        if (teleportDirection == Vector3.up)
        {
            player.GetComponent<Rigidbody2D>().gravityScale = 3;
            animator.SetBool("Jump", false);
        }
        else if (teleportDirection != Vector3.down)
        {
            animator.SetBool("MoveToPortal", false);
        }

        //fadeout
        for (float i = 1; i > 0f; i -= 0.02f)
        {
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, i);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        //테스트용으로 주석 처리
        GameManager.Instance._stageminimapManager.OnMapEntered("MAP" + teleportSet[targetID].GetComponent<Teleport>()
            .mapID);


        

        //플레이어 이동 
        player.GetComponent<PlayerController>().canMove = true;
    }

    public void MoveToPortal()
    {
        player.GetComponent<PlayerController>().canMove = false;
        //animator.SetBool("MoveToPortal", true);
    }
}