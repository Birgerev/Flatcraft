using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMenu : MonoBehaviour
{
    public void DirectConnect()
    {
        SceneManager.LoadScene("MultiplayerDirectConnectMenu");
    }

    public void Cancel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
