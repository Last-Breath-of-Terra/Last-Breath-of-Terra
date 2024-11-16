using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public GameObject player;
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
    
    [Header("Obstacle")] 
    private AudioClip[] ObstacleInitClips;
    private Dictionary<string, AudioClip> ObstacleAudioClips;
    public AudioSource[] ObstacleSources;
    private float ObstacleVolume = 1.0f;

    [Header("Footstep SFX")] 
    private Dictionary<string, AudioClip[]> footstepClipsByMap;
    

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
        

        #region FootStep Init

        footstepClipsByMap = new Dictionary<string, AudioClip[]>();
        LoadFootstepClips("AlphaTest");

        #endregion
    }

    private void LoadFootstepClips(string mapType)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>($"Audio/Footsteps/{mapType}");
        footstepClipsByMap[mapType] = clips;
    }

    private void Start()
    {
       PlayBGM("BGM1");
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

    #region Custom Sound
    public void PlayFootstepSFX(string mapType, AudioSource audioSource, bool isStopping)
    {
        AudioClip clip;
        if (isStopping)
        {
            clip = GetStoppingFootstepClipByMap(mapType);
        }
        else
        {
            clip = GetRandomFootstepClipByMap(mapType);
        }

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public AudioClip GetRandomFootstepClipByMap(string mapType)
    {
        if (footstepClipsByMap.ContainsKey(mapType))
        {
            AudioClip[] clips = footstepClipsByMap[mapType];
            int randomIndex = UnityEngine.Random.Range(0, clips.Length - 1);
            return clips[randomIndex];
        }
        return null;
    }

    public AudioClip GetStoppingFootstepClipByMap(string mapType)
    {
        if (footstepClipsByMap.ContainsKey(mapType))
        {
            AudioClip[] clips = footstepClipsByMap[mapType];
            return clips[clips.Length - 1];
        }
        return null;
    }
    #endregion

    public void StopAudio()
    {
        bgmSource.Stop();
        ambienceSource.Stop();
    }
}