using System.Collections.Generic;
using System.Threading.Tasks;
using kcp2k;
using Mirror;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkManager
{
    public List<string> prefabDirectories = new List<string>();
    public GameObject WorldManagerPrefab;

    private static Lobby _currentLobby;
    private bool _initialized;

    public static async void HostGameAsync()
    {
        //Host lobby
        HostSteamLobbyAsync();
        
        //Load game scene
        SceneManager.LoadScene("Game");
        
        MultiplayerManager multiplayerManager = CreateMultiplayerManager();

        //Await manager initialized
        while (!multiplayerManager._initialized)
        {
            await Task.Delay(100);
        }
        
        multiplayerManager.StartHost();
    }
    
    private static async void HostSteamLobbyAsync()
    {
        //Host lobby
        Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync();

        if (!lobby.HasValue)
        {
            Debug.LogError("Failed to initialize steam lobby");
            return;
        }

        Debug.Log("Successfully created steam lobby");
        
        _currentLobby = lobby.Value;
        _currentLobby.SetFriendsOnly();
    }

    private static MultiplayerManager CreateMultiplayerManager()
    {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Multiplayer Manager")).GetComponent<MultiplayerManager>();
    }

    public override void Awake()
    {
        base.Awake();
        
        //Register all network prefabs to manager
        foreach (string dir in prefabDirectories)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(dir + "/");
            foreach (GameObject prefab in prefabs)
                NetworkClient.RegisterPrefab(prefab);
        }
    }
    
    public override void Start()
    {
        base.Start();

        _initialized = true;
    }
    
    public static void StopConnection()
    {
        /*TODO
        switch (connectionMode)
        {
            case ConnectionMode.Client:
                singleton.StopClient();
                break;
            case ConnectionMode.Host:
                singleton.StopHost();
                break;
            case ConnectionMode.DedicatedServer:
                singleton.StopServer();
                break;
        }*/
        
        SceneManager.LoadScene("MainMenu");
    }
    
    public override void OnStopClient()
    {
        base.OnStopClient();

        Destroy(gameObject);
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
    }
}

public enum ConnectionMode
{
    Client,
    Host,
}