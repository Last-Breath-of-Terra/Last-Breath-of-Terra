using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    //Ambience BGM Foley SFX

    [Header("BGM")] 
    private AudioClip[] BGMInitClips;
    private Dictionary<string, AudioClip> BGMAudioClips;
    private AudioSource bgmSource;
    private float bgmVolume = 1.0f;

    [Header("Ambience")] 
    private AudioClip[] ambienceInitClips;
    private Dictionary<string, AudioClip> ambienceAudioClips;
    private AudioSource ambienceSource;
    private float ambienceVolume = 1.0f;

    [Header("SFX")] 
    private AudioClip[] SFXInitClips;
    private Dictionary<string, AudioClip> SFXAudioClips;
    public AudioSource[] sfxSources;
    private float sfxVolume = 1.0f;


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
            Debug.Log(clip.name);
            SFXAudioClips[clip.name] = clip;
        }

        #endregion
    }

    private void Start()
    {
      //  PlayBGM("bgm_opening");
      //  PlayAmbience("ambi_livingroom");
    }

    public void PlayBGM(string bgmName)
    {
        //Debug.Log(bgmName);
        if (BGMAudioClips.ContainsKey(bgmName))
        {
            bgmSource.clip = BGMAudioClips[bgmName];
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    public void PlayAmbience(string ambienceName)
    {
        Debug.Log(ambienceName);
        if (ambienceAudioClips.ContainsKey(ambienceName))
        {
            ambienceSource.clip = ambienceAudioClips[ambienceName];
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }
    }


    public void PlaySFX(string sfxName, AudioSource audioSource)
    {
        Debug.Log(SFXAudioClips.Count);
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            audioSource.volume = sfxVolume;
            audioSource.PlayOneShot(SFXAudioClips[sfxName]);
        }
    }

    public void PlaySpatialSFX(string sfxName, AudioSource audioSource, Transform soundTransform)
    {
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            Vector3 playerPosition = GameManager.Instance.player.position;
            Vector3 objectPosition = soundTransform.position;

            float panValue = Mathf.Clamp((objectPosition.x - playerPosition.x) / 2.0f, -1.0f, 1.0f);

            audioSource.panStereo = panValue;
            audioSource.volume = sfxVolume;
            audioSource.PlayOneShot(SFXAudioClips[sfxName]);
        }
    }

    public void StopAudio()
    {
        bgmSource.Stop();
        ambienceSource.Stop();
    }
}