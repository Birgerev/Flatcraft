using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class MultiplayerManager : NetworkManager
{
    //dont need singleton variable here, use "singleton" from NetworkManager
    
    public List<string> prefabDirectories = new List<string>();
    public GameObject WorldManagerPrefab;
    private bool _initialized;
    
    //TODO check settings is multiplayer enabled?
    
    public override void Awake()
    {
        base.Awake();
        
        //Register all network prefabs to manager
        /*TODO do this manually
        foreach (string dir in prefabDirectories)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(dir + "/");
            foreach (GameObject prefab in prefabs)
                NetworkClient.RegisterPrefab(prefab);
        }*/

#if !DISABLESTEAMWORKS
        RegisterSteamCallbacks();
#endif

        _initialized = true;
    }

    public static async void HostGameAsync()
    {
        //Load game scene
        Debug.Log("Loading Game Scene");
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Game");
        while (!sceneLoad.isDone)
        {
            await Task.Delay(10);
        }

        //Await manager initialized
        Debug.Log("Creating Multiplayer Manager");
        MultiplayerManager multiplayerManager = CreateMultiplayerManager();
        while (!multiplayerManager._initialized)
        {
            await Task.Delay(10);
        }
        
        //Start game server
        multiplayerManager.StartHost();
        
        //Host steam lobby
#if !DISABLESTEAMWORKS
        CreateSteamLobby();
#endif
    }

    private static MultiplayerManager CreateMultiplayerManager()
    {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Multiplayer Manager")).GetComponent<MultiplayerManager>();
    }
    
    public void StopConnection()
    {
        switch (singleton.mode)
        {
            case NetworkManagerMode.ClientOnly:
                singleton.StopClient();
                break;
            case NetworkManagerMode.Host:
                singleton.StopHost();
                break;
        }

#if !DISABLESTEAMWORKS
        LeaveSteamLobby();
        UnregisterSteamCallbacks();
#endif

        SceneManager.LoadScene("MainMenu");
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        //If game is starting on the server, Instantiate the world manager
        GameObject worldManager = Instantiate(WorldManagerPrefab);
        NetworkServer.Spawn(worldManager);
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        //Once Scene is loaded on client, send ready message to server
        if (newSceneName.Equals(onlineScene))
            NetworkClient.Ready();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        //If player still is in the game scene, the player did not disconnect manually
        //Thus load the disconnected menu
        if(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game"))
            SceneManager.LoadScene("MultiplayerDisconnectedMenu");
        
        Destroy(gameObject);
    }

    
#if !DISABLESTEAMWORKS
    public CSteamID lobbyId;
    public ELobbyType currentLobbyVisibility;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<LobbyEnter_t> lobbyEntered;
    
    private void RegisterSteamCallbacks()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnSteamLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnSteamLobbyJoined);
    }

    private void UnregisterSteamCallbacks()
    {
        lobbyCreated.Unregister();
        lobbyEntered.Unregister();
    }
    
    private void LeaveSteamLobby()
    {
        Debug.Log("Leaving steam lobby");
        SteamMatchmaking.LeaveLobby(lobbyId);
    }
    
    private static void CreateSteamLobby()
    {
        Debug.Log("Creating steam lobby");
        SteamMatchmaking.CreateLobby(GetSettingLobbyVisibility(), singleton.maxConnections);
        ((MultiplayerManager)singleton).currentLobbyVisibility = GetSettingLobbyVisibility();
        
        singleton.InvokeRepeating(nameof(UpdateLobbyVisibility), 1, 1);
    }

    private static ELobbyType GetSettingLobbyVisibility()
    {
        return SettingsManager.GetStringValue("multiplayer").Equals("friends")
            ? ELobbyType.k_ELobbyTypeFriendsOnly
            : ELobbyType.k_ELobbyTypePrivate;
    }

    public void UpdateLobbyVisibility()
    {
        //Called by Invoked Repeating
        ELobbyType settingsLobbyVisibility = GetSettingLobbyVisibility();
        
        if (currentLobbyVisibility != settingsLobbyVisibility)
        {
            SteamMatchmaking.SetLobbyType(lobbyId, settingsLobbyVisibility);
            currentLobbyVisibility = settingsLobbyVisibility;
            Debug.Log("Lobby visibility set to " + currentLobbyVisibility);
        }
    }

    public static async void JoinGameAsync(CSteamID lobbyId)
    {
        if (!SteamManager.Initialized)
            return;
        //Load game scene
        Debug.Log("Loading Game Scene");
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Game");
        while (!sceneLoad.isDone)
        {
            await Task.Delay(10);
        }

        //Await manager initialized
        Debug.Log("Creating Multiplayer Manager");
        MultiplayerManager multiplayerManager = CreateMultiplayerManager();
        while (!multiplayerManager._initialized)
        {
            await Task.Delay(10);
        }
        
        //Join steam lobby
        Debug.Log("Joining steam lobby");
        SteamMatchmaking.JoinLobby(lobbyId);
        Debug.Log("Awaiting steam response...");
        //Await the steam callback to update the lobby id
        while (multiplayerManager.lobbyId == CSteamID.Nil)
        {
            await Task.Delay(10);
        }
        
        //Get lobby owner, to use as server address
        CSteamID lobbyOwnerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
        
        //Start server connection
        multiplayerManager.networkAddress = lobbyOwnerId.ToString();
        multiplayerManager.StartClient();
    }

    private void OnSteamLobbyJoined(LobbyEnter_t callback)
    {
        Debug.Log("Steam lobby join success");
        lobbyId = (CSteamID)callback.m_ulSteamIDLobby;
    }
    
    private void OnSteamLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Steam lobby creation failed, result: " + nameof(callback.m_eResult));
            return;
        }

        lobbyId = (CSteamID)callback.m_ulSteamIDLobby;
        Debug.Log("Steam lobby creation success");
    }
#endif
}