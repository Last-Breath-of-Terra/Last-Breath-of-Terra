using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public GameObject player;

    //Ambience BGM Foley SFX
    [Header("BGM")] private AudioClip[] BGMInitClips;
    private Dictionary<string, AudioClip> BGMAudioClips;
    private AudioSource bgmSource;
    private float bgmVolume = 0.5f;

    [Header("Ambience")] private AudioClip[] ambienceInitClips;
    private Dictionary<string, AudioClip> ambienceAudioClips;
    private AudioSource ambienceSource;
    private float ambienceVolume = 1.0f;

    [Header("SFX")] private AudioClip[] SFXInitClips;
    private Dictionary<string, AudioClip> SFXAudioClips;
    public AudioSource[] sfxSources;
    private float sfxVolume = 1.0f;

    [Header("Obstacle")] private AudioClip[] ObstacleInitClips;
    private Dictionary<string, AudioClip> ObstacleAudioClips;
    public AudioSource[] ObstacleSources;
    private float ObstacleVolume = 1.0f;

    private Tween currentTween;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        AudioInit();
    }

    private void AudioInit()
    {
        #region BGM init

        BGMInitClips = Resources.LoadAll<AudioClip>("Audio/BGM");
        BGMAudioClips = new Dictionary<string, AudioClip>();
        foreach (var clip in BGMInitClips)
        {
            BGMAudioClips[clip.name] = clip;
        }

        GameObject bgmGameObject = new GameObject("BGM");
        bgmGameObject.transform.SetParent(transform);
        bgmSource = bgmGameObject.AddComponent<AudioSource>();
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        #endregion

        #region Ambience Init

        ambienceInitClips = Resources.LoadAll<AudioClip>("Audio/Ambience");
        ambienceAudioClips = new Dictionary<string, AudioClip>();
        foreach (var clip in ambienceInitClips)
        {
            ambienceAudioClips[clip.name] = clip;
        }

        GameObject ambienceGameObject = new GameObject("Ambience");
        ambienceGameObject.transform.SetParent(transform);
        ambienceSource = ambienceGameObject.AddComponent<AudioSource>();
        ambienceSource.volume = ambienceVolume;
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;

        #endregion

        #region SFX Init

        SFXInitClips = Resources.LoadAll<AudioClip>("Audio/SFX");
        SFXAudioClips = new Dictionary<string, AudioClip>();
        foreach (var clip in SFXInitClips)
        {
            SFXAudioClips[clip.name] = clip;
        }

        #endregion
    }

    private void Start()
    {
        PlayBGM("BGM1");
        PlayAmbience("ambi_livingroom");
    }

    public void PlayBGM(string bgmName)
    {
        if (BGMAudioClips.ContainsKey(bgmName))
        {
            bgmSource.clip = BGMAudioClips[bgmName];
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    public void PlayAmbience(string ambienceName)
    {
        if (ambienceAudioClips.ContainsKey(ambienceName))
        {
            ambienceSource.clip = ambienceAudioClips[ambienceName];
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }
    }


    public void PlaySFX(string sfxName, AudioSource audioSource, Transform soundTransform)
    {
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            float panValue = Mathf.Clamp((soundTransform.position.x - player.transform.position.x) / 2.0f, -1.0f, 1.0f);
            audioSource.panStereo = panValue;
            audioSource.volume = sfxVolume;
            audioSource.PlayOneShot(SFXAudioClips[sfxName]);
        }
    }

    public void PlayRandomSFX(string sfxName, AudioSource audioSource, Transform soundTransform)
    {
        int randomIndex = UnityEngine.Random.Range(1, 3);
        sfxName += randomIndex;
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            float panValue = Mathf.Clamp((soundTransform.position.x - player.transform.position.x) / 2.0f, -1.0f, 1.0f);
            audioSource.panStereo = panValue;
            audioSource.volume = sfxVolume;
            audioSource.PlayOneShot(SFXAudioClips[sfxName]);
        }
    }

    public void PanSoundLeftToRight(string sfxName, float infusionDuration)
    {
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            AudioSource audioSource = player.GetComponent<AudioSource>();
            audioSource.panStereo = -1;
            audioSource.volume = sfxVolume;
            audioSource.clip = SFXAudioClips[sfxName];
            audioSource.Play();
            currentTween = DOTween.To(() => -1f, x => audioSource.panStereo = x, 1f, infusionDuration);
        }
    }

    /*
     * player 기준으로 panStereo를 결정할 때
     */
    public void PlayPlayer(string audioName, float panValue)
    {
        currentTween.Kill();
        AudioSource audioSource = player.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.panStereo = panValue;
        audioSource.PlayOneShot(SFXAudioClips[audioName]);
    }
    public void PlayRandomPlayer(string audioName, float panValue)
    {
        int randomIndex = UnityEngine.Random.Range(1, 3);
        audioName += randomIndex;
        currentTween.Kill();
        AudioSource audioSource = player.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.panStereo = panValue;
        audioSource.PlayOneShot(SFXAudioClips[audioName]);
    }
    public void PlayCancelable(string audioName, AudioSource audioSource, Transform soundTransform)
    {
        Debug.Log("playing cancelable");
        if (SFXAudioClips.ContainsKey(audioName))
        {
            audioSource.clip = SFXAudioClips[audioName];
            float panValue = Mathf.Clamp((soundTransform.position.x - player.transform.position.x) / 2.0f, -1.0f, 1.0f);
            audioSource.panStereo = panValue;
            audioSource.Play();
        }
    }

    public void StopCancelable(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.panStereo = 0;
        //PlaySFX(audioName, audioSource, soundTransform);
    }

    public void StopAudio()
    {
        bgmSource.Stop();
        ambienceSource.Stop();
    }
}