using System.Collections.Generic;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    public static ConnectionMode connectionMode;
    public static string clientConnectionAddress = "player";
    public static int port = 630;
    public static string playerName = "player";

    public List<string> prefabDirectories = new List<string>();
    public GameObject WorldManagerPrefab;

    public override void Start()
    {
        base.Start();
        //Begin setup after entering loading scene

        //Register all network prefabs to manager
        foreach (string dir in prefabDirectories)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(dir + "/");
            foreach (GameObject prefab in prefabs)
                NetworkClient.RegisterPrefab(prefab);
        }

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
        
        Debug.Log("Starting " + connectionMode);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (connectionMode == ConnectionMode.Client)
        {
            World world = new World("multiplayer", 1);

            WorldManager.world = world;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SceneManager.LoadScene("MainMenu");
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
        if (newSceneName.Equals(onlineScene))
            NetworkClient.Ready();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        SceneManager.LoadScene("MultiplayerDisconnectedMenu");
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
    }
}

public enum ConnectionMode
{
    Client,
    Host,
    DedicatedServer
}