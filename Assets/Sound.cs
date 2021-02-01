using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
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
        var obj = new GameObject("sound " + sound);
        var source = obj.AddComponent<AudioSource>();

        var clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        var clip = clips[new System.Random().Next(0, clips.Length)];

        if (clip == null)
        {
            Debug.LogError("Sound clip not found: " + sound);
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

        var pitch = Random.Range(minPitch, maxPitch);

        source.outputAudioMixerGroup = group;
        source.clip = clip;
        obj.transform.position = loc.GetPosition();
        source.pitch = pitch;
        source.maxDistance = distance;


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