using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerDirectConnectMenu : MonoBehaviour
{
    public InputField addressField;

    public void ConnectButton()
    {
        MultiplayerManager.JoinGameAsync(addressField.text);
        LoadingMenu.Create(LoadingMenuType.ConnectServer);
    }

    public void Cancel()
    {
        Destroy(gameObject);
    }
}