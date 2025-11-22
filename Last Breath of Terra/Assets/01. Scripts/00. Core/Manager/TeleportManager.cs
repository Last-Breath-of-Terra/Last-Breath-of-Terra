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

    [SerializeField] LayerMask groundLayerMask;

    private GameObject player;
    private Animator animator;
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
        player.GetComponent<PlayerMovement>().StartTeleport();

        //※※※※※※※※※※※※※※※※※※※※※※오디오 세팅※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
        AudioManager.Instance.FadeOutBGM(1f);
        AudioManager.Instance.FadeOutAmbience(1f);

        int teleportOffset = 2;

        //포탈 들어가기 전
        if (teleportDirection == Vector3.up) //위로 갈 때
        {
            player.GetComponent<PlayerController>().setGravity(0);
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
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
        ChangeCamera(teleportSet[targetID].GetComponent<Teleport>().mapID);

        yield return new WaitForSeconds(0.2f);
        
        //포탈 나와서
        if (teleportDirection == Vector3.up)
        {
            Vector3 direction = teleportSet[targetID].GetComponent<Teleport>().isRight 
                ? Vector2.right : Vector2.left;
            
            RaycastHit2D hit;
            hit = Physics2D.Raycast(player.transform.position, direction, 3f, groundLayerMask);
            Debug.DrawRay(player.transform.position, direction * 3f, Color.red, 100f);

            Vector3 hitPoint = player.transform.position;
            while (true)
            {
                if (hit.collider == null)
                    break;
                hitPoint = new Vector3(hit.point.x, hit.point.y, 0);
                player.transform.position += Vector3.up;
                yield return new WaitForSeconds(0.01f);
                
                hit = Physics2D.Raycast(player.transform.position, direction, 3f, groundLayerMask);
                Debug.DrawRay(player.transform.position, direction.normalized * 3f, Color.blue, 100f);
            }
            Debug.Log("위로 이동 끝");

            
            Vector3 dir = direction.normalized;
            float height = 2f;
            float duration = 1f;

            //상승 후 포물선 이동 관련 수정은 여기!!!
            Vector3 target = new Vector3(hitPoint.x + direction.x * 1.5f, player.transform.position.y + height, player.transform.position.z);
            player.transform.DOJump(target, height, 1, duration)
                .SetEase(Ease.OutQuad)
                .OnStart(() =>
                {
                    player.GetComponent<PlayerController>().AnimHandler.ChangeState(PlayerAnimationHandler.AnimationState.Idle);
                })
                .OnComplete(() => Debug.Log("착지 완료"));

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
            player.GetComponent<PlayerController>().setGravity(3);
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


        player.GetComponent<PlayerMovement>().EndTeleport();

        //플레이어 이동 
        player.GetComponent<PlayerController>().canMove = true;
    }

    public void MoveToPortal()
    {
        player.GetComponent<PlayerController>().canMove = false;
    }
}