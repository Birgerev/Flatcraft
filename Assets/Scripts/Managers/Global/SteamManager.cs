using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    private const int AppID = 2070630;
    
    private void Awake() {
        try {
            Steamworks.SteamClient.Init(AppID);
            Debug.Log("Steam connection initialized");
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
        MultiplayerManager.JoinGameAsync(lobby, steamID);
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }
}
