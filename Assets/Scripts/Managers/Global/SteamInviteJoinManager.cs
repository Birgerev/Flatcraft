using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SteamInviteJoinManager : MonoBehaviour
{
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    
    private void Awake() {
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(SteamUIJoinFriend);
    }

    private void SteamUIJoinFriend(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Accepted steam UI overlay invite, joining friends lobby!");
        MultiplayerManager.JoinGameAsync(callback.m_steamIDLobby);
    }
}
