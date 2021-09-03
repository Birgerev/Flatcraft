using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerDirectConnectMenu : MonoBehaviour
{
    public InputField addressField;

    private void Update()
    {
        GameNetworkManager.serverAddress = addressField.text;
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