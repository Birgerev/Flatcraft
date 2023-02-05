using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OldMultiplayerMenu : MonoBehaviour
{
    private static string savedServersPath => Application.persistentDataPath + "\\favouritedServers.dat";
    private const int BrowseServerLoadAmount = 4;
    private const string ServerAdress = "hille.evansson.se";
    private const string Url = "http://hille.evansson.se/flatcraft/";
    private const string GetServerByIdUrl = Url + "get-dedicated-server.php";
    private const string GetServerListUrl = Url + "get-dedicated-server-list.php";
    
    public GameObject directConnectMenuPrefab;
    
    [Space][Space]
    
    public Text myServersButtonText;
    public bool showMyServers = true;
    public CanvasGroup myServersSection;
    public CanvasGroup browseServersSection;
    public Transform myServersList;
    public Transform browseServersList;
    public GameObject dedicatedServerPrefab;
    public Scrollbar browseServersScrollbar;
    
    public DedicatedServer selectedServer;

    private int browseServersIndex = 0;

    public void ToggleShowMyServers()
    {
        showMyServers = !showMyServers;
    }
    
    public void DirectConnect()
    {
        Instantiate(directConnectMenuPrefab);
    }

    public void Cancel()
    {
        Destroy(gameObject);
    }
    
    private void Start()
    {
        if (!File.Exists(savedServersPath))
            File.Create(savedServersPath);
        
        browseServersScrollbar.value = 1f;
        Refresh();
    }

    private void Update()
    {
        myServersButtonText.text = showMyServers ? "Browse Servers" : "My Servers";
        
        myServersSection.alpha = showMyServers ? 1 : 0;
        myServersSection.interactable = showMyServers;
        myServersSection.blocksRaycasts = showMyServers;
        
        browseServersSection.alpha = !showMyServers ? 1 : 0;
        browseServersSection.interactable = !showMyServers;
        browseServersSection.blocksRaycasts = !showMyServers;

        if (browseServersScrollbar.value < 0.05f && Time.time % 0.5f - Time.deltaTime <= 0)
        {
            StartCoroutine(LoadMoreServerLists());
        }
    }

    IEnumerator LoadMyServers()
    {
        string[] savedServerIds = File.ReadAllLines(savedServersPath);

        foreach (string serverId in savedServerIds)
        {
            if (serverId.Length == 0)
                continue;
            
            UnityWebRequest www = UnityWebRequest.Get(GetServerByIdUrl + "?id=" + serverId);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string serverData = www.downloadHandler.text;
                DedicatedServer server = new DedicatedServer(serverData);
                GameObject serverButtonObject = Instantiate(dedicatedServerPrefab, myServersList);
                DedicatedServerButton serverButton = serverButtonObject.GetComponent<DedicatedServerButton>();
                
                serverButton.server = server;
            }
        }
    }

    IEnumerator LoadMoreServerLists()
    {
        UnityWebRequest www = UnityWebRequest.Get(GetServerListUrl + "?startIndex=" + browseServersIndex + "&amount=" + BrowseServerLoadAmount);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string serverListData = www.downloadHandler.text;

            foreach (string serverData in serverListData.Split('|'))
            {
                if (serverData.Length == 0)
                    break;
                
                DedicatedServer server = new DedicatedServer(serverData);
                GameObject serverButtonObject = Instantiate(dedicatedServerPrefab, browseServersList);
                DedicatedServerButton serverButton = serverButtonObject.GetComponent<DedicatedServerButton>();
            
                serverButton.server = server;
            }
        }

        browseServersIndex += BrowseServerLoadAmount;
    }

    
    public void Play()
    {
        Debug.LogError("Trying to join using deprecated code");
        /*GameNetworkManager.clientConnectionAddress = ServerAdress;
        GameNetworkManager.clientConnectionAddress = selectedServer.address;
        GameNetworkManager.port = selectedServer.port;
        GameNetworkManager.connectionMode = ConnectionMode.Client;
        GameNetworkManager.StartGame();*/
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }

    public void Refresh()
    {
        browseServersIndex = 0;
        
        //Clear old servers
        foreach (DedicatedServerButton serverButton in GetComponentsInChildren<DedicatedServerButton>())
        {
            Destroy(serverButton.gameObject);
        }
        
        StartCoroutine(LoadMyServers());
        StartCoroutine(LoadMoreServerLists());
    }


    public void ServerToggleFavourite()
    {
        List<string> savedServerIds = File.ReadAllLines(savedServersPath).ToList();
        string id = selectedServer.id.ToString();

        if (savedServerIds.Contains(id))
        {
            savedServerIds.Remove(id);
        }
        else
        {
            savedServerIds.Add(id);
        }
        
        File.WriteAllLines(savedServersPath, savedServerIds);
        
        Refresh();
    }
}