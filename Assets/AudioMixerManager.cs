using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager instance;
    public AudioMixer mixer;

    public float debugVolume;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp01(debugVolume) + 0.0001f) * 20);
    }
}
