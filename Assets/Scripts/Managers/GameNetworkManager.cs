using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameNetworkManager : Mirror.NetworkManager
{
    public static bool isHost;
    public static string serverAddress = "player";
    public static string PlayerName = "player";
    
    public List<string> prefabDirectories = new List<string>();
    public GameObject WorldManagerPrefab;
    
    public override void Start()
    {
        base.Start();

        foreach (string dir in prefabDirectories)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(dir + "/");
            foreach (GameObject prefab in prefabs)
            {
                ClientScene.RegisterPrefab(prefab);
            }
        }

        if (isHost)
        {
            StartHost();
        }
        else
        {
            networkAddress = serverAddress;
            StartClient();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!isHost)
        {
            World world = new World("multiplayer", 1);
            world.versionId = VersionController.CurrentVersionId;

            WorldManager.world = world;
        }
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
        {
            ClientScene.Ready(NetworkClient.connection);
        }
    }
}
