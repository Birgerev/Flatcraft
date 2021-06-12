using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerDirectConnectMenu : MonoBehaviour
{
    public InputField addressField;
    public InputField nameField;

    private void Update()
    {
        GameNetworkManager.serverAddress = addressField.text;
        GameNetworkManager.playerName = nameField.text;
    }

    public void ConnectButton()
    {
        GameNetworkManager.isHost = false;
        SceneManager.LoadScene("Game");
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }

    public void Cancel()
    {
        SceneManager.LoadScene("MultiplayerMenu");
    }
}