using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

     [Header("Background Music Clips")]
     [SerializeField] private SoundSO crowdNoise;
     [SerializeField] private SoundSO gameBGMusic;
     
     [Header("SFX Clips")]
     [SerializeField] private SoundSO[] soundClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene change event
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            PlayMusic(gameBGMusic.clip);
        }
        else
        {
            PlayMusic(crowdNoise.clip);
        }
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        musicSource.clip = clip;
        musicSource.volume = volume;
        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void PlaySFX(string sfxName)
    {
        SoundSO sound = null;
        foreach (var s in soundClips)
        {
            if (s != null && s.name == sfxName)
            {
                sound = s;
                break;
            }
        }
        if (sound != null)
        {
            sfxSource.clip = sound.clip;
            sfxSource.volume = sound.volume;
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{sfxName}' not found in soundClips array.");
        }
    }

    public void PlaySound(SoundSO sound)
    {
        sfxSource.clip = sound.clip;
        sfxSource.volume = sound.volume;
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }
}