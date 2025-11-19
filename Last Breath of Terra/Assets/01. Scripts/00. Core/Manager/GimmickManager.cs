using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : Singleton<GimmickManager>
{
    public LifeInfuserSO lifeInfuserSO;
    public float gimmicVolume = 1.0f;
    private Coroutine _coroutine;

    [Header("SFX")] private AudioClip[] gimmickSFXInitClips;
    private Dictionary<string, AudioClip> gimmickSFXAudioClips;
    private float sfxVolume = 1.0f;

    protected override void Awake()
    {
        gimmickSFXInitClips = Resources.LoadAll<AudioClip>("Audio/Gimmick");
        gimmickSFXAudioClips = new Dictionary<string, AudioClip>();
        foreach (var clip in gimmickSFXInitClips)
        {
            gimmickSFXAudioClips[clip.name] = clip;
        }
    }


    public void ChangeLifeInfuserUISize()
    {
        if (lifeInfuserSO != null)
        {
            lifeInfuserSO.SetUIForInfuserStatus(true);
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        Debug.Log("Hide after delay");
        yield return new WaitForSeconds(UIManager.Instance.hideDelay + lifeInfuserSO.uiTweenDuration);
        if (lifeInfuserSO != null)
        {
            Debug.Log("Player entered");
            lifeInfuserSO.SetUIForInfuserStatus(false);
        }

        _coroutine = null;
    }


    public float PlayGimmickSFX(string sfxName, GameObject gameObject, bool isRandom)
    {
        AudioSource _audioSource = gameObject.GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (isRandom)
        {
            int randomIndex = UnityEngine.Random.Range(1, 3);
            sfxName += "_0" + randomIndex;
        }

        AudioClip _audioClip = gimmickSFXAudioClips[sfxName];
        if (_audioClip != null)
        {

            float panValue = Mathf.Clamp(
                (gameObject.transform.position.x - GameManager.Instance.playerTr.position.x) / 2.0f,
                -1.0f, 1.0f);
            _audioSource.panStereo = panValue;
            _audioSource.volume = gimmicVolume;
            _audioSource.PlayOneShot(_audioClip);
        }

        return _audioClip.length;
    }

    public void StopGimmickSFX()
    {
        AudioSource _audioSource = gameObject.GetComponent<AudioSource>();
        if(_audioSource != null)
            _audioSource.Stop();
    }
}