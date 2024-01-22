using UnityEngine;
using UnityEngine.UI;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class MultiplayerMenu : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject friendsWorldButtonPrefab;
    
    [Header("UI Elements")]
    public Button playButton;
    public Button cancelButton;
    public Transform friendWorldsContainer;

    public void Cancel()
    {
        Destroy(gameObject);
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        cancelButton.onClick.AddListener(Cancel);
        
#if !DISABLESTEAMWORKS
        playButton.onClick.AddListener(Play);
        InvokeRepeating(nameof(Refresh), 0, 5);
#endif
    }

#if !DISABLESTEAMWORKS
    public CSteamID selectedLobby;


    public void Refresh()
    {
        if (!SteamManager.Initialized)
            return;
        
        //Clear old servers
        foreach (FriendsWorldButton friendsWorldButton in GetComponentsInChildren<FriendsWorldButton>())
        {
            Destroy(friendsWorldButton.gameObject);
        }
        
        //List of all Friends
        EFriendFlags friendFlags = EFriendFlags.k_EFriendFlagImmediate;
        int friendCount = SteamFriends.GetFriendCount(friendFlags);//TODO fetch lobbies with steamMatchmaking
        
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
#endif
}
