using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    public Lobby? selectedLobby;

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
        
        //InvokeRepeating(nameof(Refresh), 0, 2);
    }


    public void Refresh()
    {
        //Clear old servers
        foreach (FriendsWorldButton friendsWorldButton in GetComponentsInChildren<FriendsWorldButton>())
        {
            Destroy(friendsWorldButton.gameObject);
        }
        
        //List of all Friends
        var friends = SteamFriends.GetFriends();

        //Sort out a list of joinable lobbies from friends
        List<Lobby> joinableLobbies = new List<Lobby>();
        foreach (var friend in friends)
        {
            if(friend.IsMe)
                continue;
            if(!friend.IsPlayingThisGame)
                continue;
            if(!friend.GameInfo.HasValue)
                continue;
            if(!friend.GameInfo.Value.Lobby.HasValue)
                continue;
            
            joinableLobbies.Add(friend.GameInfo.Value.Lobby.Value);
        }

        //Create joinable buttons for each friend in a lobby
        foreach (Lobby lobby in joinableLobbies)
        {
            GameObject buttonGameObject = Instantiate(friendsWorldButtonPrefab, friendWorldsContainer);
            FriendsWorldButton button = buttonGameObject.GetComponent<FriendsWorldButton>();

            button.lobby = lobby;
        }
    }
    
    public void Play()
    {
        //Dont play if lobby hasn't been assigned a value
        if(!selectedLobby.HasValue)
            return;
        
        //TODO maybe selectedLobby.Join()
        GameNetworkManager.clientConnectionAddress = selectedLobby.Owner.Id.ToString();
        GameNetworkManager.connectionMode = ConnectionMode.Client;
        GameNetworkManager.StartGame();
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }
    
    public void Cancel()
    {
        Destroy(gameObject);
    }
}
