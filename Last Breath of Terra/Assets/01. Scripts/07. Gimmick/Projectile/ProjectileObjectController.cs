using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileObstacleController : MonoBehaviour
{
    public int requiredDestroyedCount = 3;
    public float respawnTime = 5f;
    [SerializeField] private float destroyDelay = 0.5f;

    private int currentDestroyedCount = 0;
    private bool isDestroyed = false;

    public bool isStage1 = false;
    
    public GameObject obstacleVisual;
    private Collider2D obstacleCollider;

    [Header("Visual")]
    [SerializeField] private GameObject[] stageObjects; // [0]=기본, [1]=알파변경, [2]=항상 표시

    private SpriteRenderer stage1Renderer;
    private Coroutine alphaLerpCoroutine;

    private void Awake()
    {
        obstacleCollider = GetComponent<Collider2D>();

        if (stageObjects.Length >= 3)
        {
            stageObjects[0].SetActive(true);

            stage1Renderer = stageObjects[1].GetComponent<SpriteRenderer>();
            if (stage1Renderer != null)
            {
                Color c = stage1Renderer.color;
                c.a = 0f;
                stage1Renderer.color = c;
            }
            stageObjects[1].SetActive(true);
            stageObjects[2].SetActive(true);
        }
    }

    public void OnOneProjectileDestroyed()
    {
        if (isDestroyed) return;

        switch (currentDestroyedCount)
        {
            case 0:
                if(isStage1)
                    GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimmick_RockBreakNotice_01", gameObject, true);
                else
                    GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimmick_IceRockBreakNotice_01", gameObject, true);

                Debug.Log("play destroy sound 1");
                break;
            case 1:
                if(isStage1)
                    GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimmick_RockBreak_02_01", gameObject, false);
                else
                    GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimmick_IceRockBreak_02", gameObject, true);
                Debug.Log("play destroy sound 2");
                break;
            /*
            case 2:
                GimmickManager.Instance.PlayGimmickSFX("Sfx_Gimmick_RockFragment_03", gameObject, false);
                Debug.Log("play destroy sound 3");
                break;*/
            
        }
        currentDestroyedCount++;

        if (stage1Renderer != null)
        {
            byte[] alphaSteps = { 100, 150, 255 };
            int index = Mathf.Clamp(currentDestroyedCount - 1, 0, alphaSteps.Length - 1);
            float targetAlpha = alphaSteps[index] / 255f;

            if (alphaLerpCoroutine != null)
                StopCoroutine(alphaLerpCoroutine);

            alphaLerpCoroutine = StartCoroutine(LerpAlpha(stage1Renderer, targetAlpha, 0.5f));

            if (alphaSteps[index] == 255)
            {
                // 알파 255가 되면 1초 기다렸다가 색을 (0,0,0)으로 변화
                StartCoroutine(WaitAndFadeToBlack());
            }
        }

        if (currentDestroyedCount >= requiredDestroyedCount)
        {
            StartCoroutine(DestroyObstacleWithDelay());
        }
    }

    private IEnumerator LerpAlpha(SpriteRenderer renderer, float targetAlpha, float duration)
    {
        float time = 0f;
        Color startColor = renderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (time < duration)
        {
            time += Time.deltaTime;
            renderer.color = Color.Lerp(startColor, endColor, time / duration);
            yield return null;
        }

        renderer.color = endColor;
    }

    private IEnumerator WaitAndFadeToBlack()
    {
        yield return new WaitForSeconds(1f);

        float time = 0f;
        float duration = 1f;
        Color startColor = stage1Renderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // ✅ 알파를 0으로

        while (time < duration)
        {
            time += Time.deltaTime;
            stage1Renderer.color = Color.Lerp(startColor, targetColor, time / duration);
            yield return null;
        }

        stage1Renderer.color = targetColor;
    }

    private IEnumerator DestroyObstacleWithDelay()
    {
        isDestroyed = true;

        yield return new WaitForSeconds(destroyDelay);

        if (obstacleVisual != null)
            obstacleVisual.SetActive(false);

        if (obstacleCollider != null)
            obstacleCollider.enabled = false;

        var playerInput = GameManager.Instance.playerTr.gameObject.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.FindActionMap("Gimmick").Disable();
            playerInput.actions.FindActionMap("Player").Enable();
        }

        StartCoroutine(RespawnObstacleAfterDelay());
    }

    IEnumerator RespawnObstacleAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        isDestroyed = false;
        currentDestroyedCount = 0;

        if (obstacleVisual != null)
            obstacleVisual.SetActive(true);

        if (obstacleCollider != null)
            obstacleCollider.enabled = true;

        if (stageObjects.Length >= 3)
        {
            stageObjects[0].SetActive(false); // 처음에는 꺼져 있음
            stageObjects[1].SetActive(true);
            stageObjects[2].SetActive(true);

            if (stage1Renderer != null)
            {
                stage1Renderer.color = new Color(1f, 1f, 1f, 0f); // 알파 0부터 시작
                yield return StartCoroutine(LerpAlpha(stage1Renderer, 1f, 1f)); // 0 → 255

                stageObjects[0].SetActive(true); // 기본 오브젝트 다시 켬

                yield return StartCoroutine(LerpAlpha(stage1Renderer, 0f, 1f)); // 255 → 0
            }
        }

        var spawner = GetComponentInParent<ProjectileSpawner>();
        spawner.TryResumeSpawn();
    }

    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}
