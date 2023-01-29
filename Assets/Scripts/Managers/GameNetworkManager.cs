using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    public static ConnectionMode connectionMode;
    public static string clientConnectionAddress = "Not Assigned";
    public static int port = 630;
    public static string playerName = "Steve";

    public List<string> prefabDirectories = new List<string>();
    public GameObject WorldManagerPrefab;

    public override void Start()
    {
        base.Start();
        
        //Register all network prefabs to manager
        foreach (string dir in prefabDirectories)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(dir + "/");
            foreach (GameObject prefab in prefabs)
                NetworkClient.RegisterPrefab(prefab);
        }
        
        Debug.Log("Starting connection type: " + connectionMode);

        //Start Connection based on selected connection mode
        switch(connectionMode)
        {
            case ConnectionMode.Client:
                networkAddress = clientConnectionAddress;
                GetComponent<KcpTransport>().Port = (ushort)port;
                StartClient();
                break;
            case ConnectionMode.Host:
                StartHost();
                break;
            case ConnectionMode.DedicatedServer:
                GetComponent<KcpTransport>().Port = (ushort)port;
                StartServer();
                break;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        Destroy(gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

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

    public static void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public static void Disconnect()
    {
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
        }
        
        SceneManager.LoadScene("MainMenu");
    }
}

public enum ConnectionMode
{
    Client,
    Host,
    DedicatedServer
}