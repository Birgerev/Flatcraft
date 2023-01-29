using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
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

        SteamFriends.OnGameLobbyJoinRequested += SteamUIJoinFriend;
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

    private void SteamUIJoinFriend(Lobby lobby, SteamId steamID)
    {
        GameNetworkManager.clientConnectionAddress = steamID.Value.ToString();
        
        GameNetworkManager.connectionMode = ConnectionMode.Client;
        GameNetworkManager.StartGame();
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }
}
