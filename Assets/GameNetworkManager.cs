using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameNetworkManager : Mirror.NetworkManager
{
    public List<string> prefabDirectories = new List<string>();
    public static string PlayerName = "player";
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
    }
    public override void OnStartClient()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        
        World world = new World("multiplayer", 1);
        world.versionId = VersionController.CurrentVersionId;
        world.SaveData();
        
        WorldManager.world = world;
        
        base.OnStartClient();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        
        if (sceneName.Equals(onlineScene))
        {
            GameObject worldManager = Instantiate(WorldManagerPrefab);
            NetworkServer.Spawn(worldManager);
            ClientScene.Ready(NetworkClient.connection);
        }
    }
    
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if (newSceneName.Equals(onlineScene))
        {
            ClientScene.Ready(NetworkClient.connection);
        }
    }

    public void ConnectButton()
    {
        StartClient();
    }

    public void HostButton()
    {
        StartHost();
    }

    public void ChangeName(string name)
    {
        PlayerName = name;
    }

    public void ChangeAdress(string ip)
    {
        networkAddress = ip;
    }
}
