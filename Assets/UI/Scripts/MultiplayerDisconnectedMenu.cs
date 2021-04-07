using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerDisconnectedMenu : MonoBehaviour
{
    public void BackToServerList()
    {
        SceneManager.LoadScene("MultiplayerMenu");
    }
}
