﻿using Mirror;
using UnityEngine;
using UnityEngine.Audio;

public class Sound : NetworkBehaviour
{
    public static Sound instance;
    public AudioMixerGroup blocksGroup;

    public bool enabled;
    public AudioMixerGroup entitiesGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup weatherGroup;

    public void Start()
    {
        instance = this;
    }

    public static bool Exists(string sound)
    {
        var clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        
        return (clips.Length > 0);
    }
    
    public static void Play(Location loc, string sound, SoundType type)
    {
        Play(loc, sound, type, 1, 1);
    }

    public static void Play(Location loc, string sound, SoundType type, float minPitch, float maxPitch)
    {
        Play(loc, sound, type, minPitch, maxPitch, 15);
    }

    public static void Play(Location loc, string sound, SoundType type, float minPitch, float maxPitch, float distance)
    {
        if (instance == null)
        {
            Debug.LogError("Sound manager instance not set, cant play sound: " + sound);
            return;
        }
        
        var clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        if (clips.Length == 0)
        {
            Debug.LogError("Sound clip not found: " + sound);
            return;
        }
        
        int soundIndex = new System.Random().Next(0, clips.Length);
        var pitch = Random.Range(minPitch, maxPitch);
        
        instance.ClientsPlay(loc, sound, soundIndex, type, pitch, distance);
    }

    [ClientRpc]
    public void ClientsPlay(Location loc, string sound, int soundIndex, SoundType type, float pitch, float distance)
    {
        var obj = new GameObject("sound " + sound);
        var source = obj.AddComponent<AudioSource>();
        var clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        var clip = clips[soundIndex];
        
        AudioMixerGroup group = null;
        switch (type)
        {
            case SoundType.Music:
                group = instance.musicGroup;
                break;
            case SoundType.Weather:
                group = instance.weatherGroup;
                break;
            case SoundType.Blocks:
                group = instance.blocksGroup;
                break;
            case SoundType.Entities:
                group = instance.entitiesGroup;
                break;
        }

        source.spatialBlend = 1;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;
        source.outputAudioMixerGroup = group;
        source.clip = clip;
        obj.transform.position = loc.GetPosition();
        source.pitch = pitch;
        source.maxDistance = distance;
        source.dopplerLevel = 0;
        DontDestroyOnLoad(obj);

        source.Play();

        Destroy(obj, clip.length + 1);
    }
}

public enum SoundType
{
    Music,
    Weather,
    Blocks,
    Entities,
    Menu
}