using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    //[HideInInspector] public Dictionary<int, string> mapAmbienceDict;

    private ScenesManager scenesManager;

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
        AudioInit();
        scenesManager = new ScenesManager();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayBGMForCurrentScene();
        //PlayAmbience("ambi_livingroom");
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        scenesManager.UpdateCurrentSceneType();
        PlayBGMForCurrentScene();
        UpdatePlayerAuidoSettingsByScene();
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

    //씬 전환 시 BGM 변경
    public void PlayBGMForCurrentScene()
    {
        SCENE_TYPE currentScene = scenesManager.GetCurrentSceneType();

        Debug.Log(currentScene);

        string bgmName = "";

        switch (currentScene)
        {
            case SCENE_TYPE.Title:
                bgmName = "BGM_Title";
                break;
            case SCENE_TYPE.Tutorial:
                bgmName = "BGM_Tutorial";
                break;
            case SCENE_TYPE.Stage1:
                bgmName = "BGM_Stage1_Map0";
                break;
            case SCENE_TYPE.Stage2:
                bgmName = "BGM_Stage2_Map0";
                break;
            default:
                bgmName = "BGM_Default";
                break;
        }

        PlayBGM(bgmName);
    }

    public void SwitchAmbienceAndBGM(int mapID)
    {
        SCENE_TYPE currentScene = scenesManager.GetCurrentSceneType();

        string ambienceName = "Amb_Default";
        string bgmName = "BGM_Default";

        switch (currentScene)
        {
            case SCENE_TYPE.Stage1:
                ambienceName = "Amb_Stage1_Map" + mapID;
                break;
            case SCENE_TYPE.Stage2:
                ambienceName = "Amb_Stage2_Map" + mapID;
                bgmName = "BGM_Stage2_Map" + mapID;
                break;
        }
        
        if (currentScene == SCENE_TYPE.Stage1)
        {
            if (ambienceAudioClips.ContainsKey(ambienceName))
            {
                FadeInAmbience(1f);
                PlayAmbience(ambienceName);
            }
            else
            {
                FadeInBGM(1f);
                PlayBGM("BGM_Stage1_Map0");
            }
        }
        else
        {
            if (ambienceAudioClips.ContainsKey(ambienceName))
            {
                PlayAmbience(ambienceName);
                FadeInAmbience(1f);
            }

            if (BGMAudioClips.ContainsKey(bgmName))
            {
                PlayBGM(bgmName);
                FadeInBGM(1f);
            }
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

    public void FadeOutBGM(float duration)
    {
        DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 0f, duration);
    }

    public void FadeInBGM(float duration)
    {
        DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 0.5f, duration);
    }

    public void FadeOutAmbience(float duration)
    {
        DOTween.To(() => ambienceSource.volume, x => ambienceSource.volume = x, 0f, duration);
    }

    public void FadeInAmbience(float duration)
    {
        DOTween.To(() => ambienceSource.volume, x => ambienceSource.volume = x, 1f, duration);
    }

    public void PlaySFX(string sfxName, AudioSource audioSource, Transform soundTransform)
    {
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            float panValue = Mathf.Clamp((soundTransform.position.x - GameManager.Instance.playerTr.position.x) / 2.0f,
                -1.0f, 1.0f);
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
            float panValue = Mathf.Clamp((soundTransform.position.x - GameManager.Instance.playerTr.position.x) / 2.0f,
                -1.0f, 1.0f);
            audioSource.panStereo = panValue;
            audioSource.volume = sfxVolume;
            audioSource.PlayOneShot(SFXAudioClips[sfxName]);
        }
    }

    public void PanSoundLeftToRight(string sfxName, float infusionDuration)
    {
        if (SFXAudioClips.ContainsKey(sfxName))
        {
            AudioSource audioSource = GameManager.Instance.playerTr.gameObject.GetComponent<AudioSource>();
            audioSource.panStereo = -1;
            audioSource.volume = sfxVolume;
            audioSource.clip = SFXAudioClips[sfxName];
            audioSource.Play();
            currentTween = DOTween.To(() => -1f, x => audioSource.panStereo = x, 1f, infusionDuration);
        }
    }

    /*
     * 씬 & 맵에 따라 플레이어 오디오 세팅
     */
    public void UpdatePlayerAuidoSettingsByMap(int mapID)
    {/*
        if (mapAmbienceDict.ContainsKey(mapID))
        {
            GameManager.Instance.playerTr.GetComponent<AudioChorusFilter>().enabled = true;
            GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().minDistance = 40f;
            GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().maxDistance = 60f;
        }
        else
        {
            GameManager.Instance.playerTr.GetComponent<AudioChorusFilter>().enabled = false;
            GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().minDistance = 10f;
            GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().maxDistance = 15f;
        }
        */
    }

    public void UpdatePlayerAuidoSettingsByScene()
    {
        SCENE_TYPE currentScene = scenesManager.GetCurrentSceneType();

        switch (currentScene)
        {
            case SCENE_TYPE.Tutorial:
                GameManager.Instance.playerTr.GetComponent<AudioChorusFilter>().enabled = true;
                GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().minDistance = 40f;
                GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().maxDistance = 60f;
                break;
            case SCENE_TYPE.Stage1:
                GameManager.Instance.playerTr.GetComponent<AudioChorusFilter>().enabled = false;
                GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().minDistance = 10f;
                GameManager.Instance.playerTr.GetComponent<AudioReverbZone>().maxDistance = 15f;
                break;
            default:
                break;
        }
    }

    /*
     * player 기준으로 panStereo를 결정할 때
     */
    public void PlayPlayer(string audioName, float panValue)
    {
        currentTween.Kill();
        AudioSource audioSource = GameManager.Instance.playerTr.gameObject.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.panStereo = panValue;
        audioSource.PlayOneShot(SFXAudioClips[audioName]);
    }

    public void PlayRandomPlayer(string audioName, float panValue)
    {
        int randomIndex = UnityEngine.Random.Range(1, 3);
        audioName += randomIndex;
        currentTween.Kill();
        AudioSource audioSource = GameManager.Instance.playerTr.gameObject.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.panStereo = panValue;
        Debug.Log(audioName);
        audioSource.PlayOneShot(SFXAudioClips[audioName]);
    }

    public void PlayCancelable(string audioName, AudioSource audioSource, Transform soundTransform)
    {
        Debug.Log("playing cancelable");
        if (SFXAudioClips.ContainsKey(audioName))
        {
            audioSource.clip = SFXAudioClips[audioName];
            float panValue = Mathf.Clamp((soundTransform.position.x - GameManager.Instance.playerTr.position.x) / 2.0f,
                -1.0f, 1.0f);
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