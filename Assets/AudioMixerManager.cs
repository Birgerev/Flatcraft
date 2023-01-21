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
        float musicValue = (float)int.Parse(SettingsManager.Values["soundCategory_music"]) / 100;
        
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicValue + 0.00001f) * 20);
    }
}
