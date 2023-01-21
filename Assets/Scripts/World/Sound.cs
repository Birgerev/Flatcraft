using Mirror;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

public class Sound : NetworkBehaviour
{
    public static Sound instance;

    public void Start()
    {
        instance = this;
    }

    [Server]
    public static bool Exists(string sound)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);

        return clips.Length > 0;
    }

    [Server]
    public static void Play(Location loc, string sound, SoundType type)
    {
        Play(loc, sound, type, 1, 1);
    }

    [Server]
    public static void Play(Location loc, string sound, SoundType type, float minPitch, float maxPitch)
    {
        Play(loc, sound, type, minPitch, maxPitch, 15, true);
    }

    [Server]
    public static void Play(Location loc, string sound, SoundType type, float minPitch, float maxPitch, float distance)
    {
        Play(loc, sound, type, minPitch, maxPitch, distance, true);
    }

    [Server]
    public static void Play(Location loc, string sound, SoundType type, float minPitch, float maxPitch, float distance
        , bool spacialPanning)
    {
        if (instance == null)
        {
            Debug.LogError("Sound manager instance not set, cant play sound: " + sound);
            return;
        }

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        if (clips.Length == 0)
        {
            Debug.LogError("Sound clip not found: " + sound);
            return;
        }

        int soundIndex = new Random().Next(0, clips.Length);
        float pitch = UnityEngine.Random.Range(minPitch, maxPitch);

        instance.PlayOnClients(loc, sound, soundIndex, type, pitch, distance, spacialPanning);
    }

    [ClientRpc]
    public void PlayOnClients(Location loc, string sound, int soundIndex, SoundType type, float pitch, float distance
        , bool spacialPanning)
    {
        PlayLocal(loc, sound, soundIndex, type, pitch, distance, spacialPanning);
    }

    public static GameObject PlayLocal(Location loc, string sound, int soundIndex, SoundType type, float pitch, float distance
        , bool spacialPanning)
    {
        GameObject obj = new GameObject("sound " + sound);
        AudioSource source = obj.AddComponent<AudioSource>();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds/" + sound);
        AudioClip clip = clips[soundIndex];
        
        AudioMixer mixer = AudioMixerManager.instance.mixer;
        AudioMixerGroup group = mixer.FindMatchingGroups(type.ToString())[0];
        
        source.spatialBlend = spacialPanning ? 1 : 0;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;
        source.outputAudioMixerGroup = group;
        source.clip = clip;
        obj.transform.position = loc.GetPosition();
        source.pitch = pitch;
        source.maxDistance = distance;
        source.dopplerLevel = 0;

        source.Play();

        Destroy(obj, clip.length + 1);
        return obj;
    }

    [Server]
    public static void DestroySounds(Location loc, float distance)
    {
        instance.DestroySoundsOnClients(loc, distance);
    }

    [ClientRpc]
    public void DestroySoundsOnClients(Location loc, float distance)
    {
        Vector2 position = loc.GetPosition();
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audio in audioSources)
        {
            Vector2 audioPos = audio.transform.position;
            if (Vector2.Distance(audioPos, position) <= distance)
                Destroy(audio.gameObject);
        }
    }
}

public enum SoundType
{
    Music
    , Weather
    , Block
    , Entities
    , Menu
}