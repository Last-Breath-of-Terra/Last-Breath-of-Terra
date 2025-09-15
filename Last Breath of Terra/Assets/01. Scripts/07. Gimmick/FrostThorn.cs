using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class FrostThornTrap : MonoBehaviour
{
    [Header("Debuff Settings")]
    [Tooltip("이동속도 감소 비율 (0.5 = 50%)")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;
    
    [Tooltip("디버프 지속 시간 (초)")]
    public float slowDuration = 3f;

    [Header("Sound Settings")]
    [SerializeField] private string sfxName = "ice_break";

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            

            var movement = collision.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                string sfxName = "Sfx_Gimmick_FrostRoad01";
                if (SceneManager.GetActiveScene().name == "Stage1")
                    sfxName = "Sfx_Gimmick_forestthornRoad01";
                GimmickManager.Instance.PlayGimmickSFX(sfxName, gameObject);
                
                movement.ApplySpeedDebuff(slowMultiplier, slowDuration);
            }
            

            //var playerAudioSource = GameManager.Instance.playerTr.GetComponent<AudioSource>();
            //AudioManager.Instance.PlayRandomSFX(sfxName, playerAudioSource, transform);
        }
    }
}
