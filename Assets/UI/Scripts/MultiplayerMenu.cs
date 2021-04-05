using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    public InputField addressField;
    public InputField nameField;
    
    public void ConnectButton()
    {
        GameNetworkManager.isHost = false;
        SceneManager.LoadScene("Game");
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }

    private void Update()
    {
        GameNetworkManager.serverAddress = addressField.text;
        GameNetworkManager.playerName = nameField.text;
    }
    
    public void Cancel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}