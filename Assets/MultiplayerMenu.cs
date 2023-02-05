using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    public CSteamID selectedLobby;

    [Header("Prefabs")]
    public GameObject friendsWorldButtonPrefab;
    
    [Header("UI Elements")]
    public Button playButton;
    public Button cancelButton;
    public Transform friendWorldsContainer;
    
    // Start is called before the first frame update
    void Awake()
    {
        playButton.onClick.AddListener(Play);
        cancelButton.onClick.AddListener(Cancel);
        
        InvokeRepeating(nameof(Refresh), 0, 5);
    }


    public void Refresh()
    {
        //Clear old servers
        foreach (FriendsWorldButton friendsWorldButton in GetComponentsInChildren<FriendsWorldButton>())
        {
            Destroy(friendsWorldButton.gameObject);
        }
        
        //List of all Friends
        EFriendFlags friendFlags = EFriendFlags.k_EFriendFlagImmediate;
        int friendCount = SteamFriends.GetFriendCount(friendFlags);
        
        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendId = SteamFriends.GetFriendByIndex(i, friendFlags);
            FriendGameInfo_t gameInfo;
            
            //Is friend playing a game?
            if(!SteamFriends.GetFriendGamePlayed(friendId, out gameInfo)) continue;
            
            //playing this game?
            if(gameInfo.m_gameID.m_GameID != SteamManager.AppId) continue;
            
            //In a lobby?
            CSteamID lobbyId = gameInfo.m_steamIDLobby;
            if(lobbyId == CSteamID.Nil) continue;

            CreateWorldButton(friendId, lobbyId);
        }
    }

    private void CreateWorldButton(CSteamID friendId, CSteamID lobbyId)
    {
        GameObject buttonGameObject = Instantiate(friendsWorldButtonPrefab, friendWorldsContainer);
        FriendsWorldButton button = buttonGameObject.GetComponent<FriendsWorldButton>();

        button.friendId = friendId;
        button.lobbyId = lobbyId;
    }
    
    public void Play()
    {
        //Dont play if lobby hasn't been assigned a value
        if(selectedLobby == CSteamID.Nil)
            return;
        
        MultiplayerManager.JoinGameAsync(selectedLobby);
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }
    
    public void Cancel()
    {
        Destroy(gameObject);
    }
}
