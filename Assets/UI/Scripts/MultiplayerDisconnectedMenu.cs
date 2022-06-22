using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerDisconnectedMenu : MonoBehaviour
{
    public void BackToServerList()
    {
        SceneManager.LoadScene("MainMenu");
    }
}