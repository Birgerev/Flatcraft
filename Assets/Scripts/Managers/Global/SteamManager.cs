using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public const int AppID = 2070630;
    
    private void Awake() {
        try {
            Steamworks.SteamClient.Init(AppID);
            Debug.LogError("Steam connection initialized");
        } catch (System.Exception e){
            Debug.LogError("Steam manager connection failed");
        }
    }
    
    private void OnApplicationQuit()
    {
        Debug.LogError("Steam connection closing...");
        Steamworks.SteamClient.Shutdown();
    }
    
    private void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }
}
