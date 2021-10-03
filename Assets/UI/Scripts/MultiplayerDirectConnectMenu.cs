using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerDirectConnectMenu : MonoBehaviour
{
    public InputField addressField;

    private void Update()
    {
        GameNetworkManager.clientConnectionAddress = addressField.text;
    }

    public void ConnectButton()
    {
        GameNetworkManager.connectionMode = ConnectionMode.Client;
        SceneManager.LoadScene("Game");
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }

    public void Cancel()
    {
        SceneManager.LoadScene("MultiplayerMenu");
    }
}