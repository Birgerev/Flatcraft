using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager instance;
    public AudioMixer mixer;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        AdjustChannelVolumeToSettingsValue("soundCategory_master", "MasterVolume");
        AdjustChannelVolumeToSettingsValue("soundCategory_entities", "EntitiesVolume");
        AdjustChannelVolumeToSettingsValue("soundCategory_block", "BlockVolume");
        AdjustChannelVolumeToSettingsValue("soundCategory_music", "MusicVolume");
    }

    private void AdjustChannelVolumeToSettingsValue(string settingsKey, string channelVolumeName)
    {
        float channelVolume01 = (float)SettingsManager.GetIntValue(settingsKey) / 100;
        float dbVolumeValue = Mathf.Log10(channelVolume01 + 0.00001f) * 20;
        
        mixer.SetFloat(channelVolumeName, dbVolumeValue);
    }
}
