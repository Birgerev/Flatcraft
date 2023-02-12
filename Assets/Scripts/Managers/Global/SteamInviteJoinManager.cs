using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamInviteJoinManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    
    private void Start() {
        if (SteamManager.Initialized)
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(SteamUIJoinFriend);
    }

    private void SteamUIJoinFriend(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Accepted steam UI overlay invite, joining friends lobby!");
        MultiplayerManager.JoinGameAsync(callback.m_steamIDLobby);
    }
#endif
}
