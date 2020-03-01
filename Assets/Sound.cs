using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
{
    public static Sound instance;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup weatherGroup;
    public AudioMixerGroup blocksGroup;
    public AudioMixerGroup entitiesGroup;


    public void Start()
    {
        instance = this;
    }


    public static void Play(Location loc, string sound, SoundType type, float pitch)
    {
        GameObject obj = new GameObject("sound "+sound);
        AudioSource source = obj.AddComponent<AudioSource>();

        AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sound);

        if(clip == null)
        {
            Debug.LogError("Sound clip not found: "+sound);
            return;
        }
        
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

        source.outputAudioMixerGroup = group;
        source.clip = clip;
        obj.transform.position = loc.getPosition();
        source.pitch = pitch;


        source.Play();

        Destroy(obj, clip.length+1);
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
