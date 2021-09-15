using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Boot : MonoBehaviour
{
    void Start()
    {
        if (DedicatedServerManager.DedicatedServerCheck())
            return;
        CreateNameCheck();
        
        SceneManager.LoadScene("MainMenu");
    }

    private void CreateNameCheck()
    {
        string testingNamePath = Application.dataPath + "/../testingProfile.dat";
        if(!File.Exists(testingNamePath))
        {
            SceneManager.LoadScene("TestingCreateName");
            return;
        }
        GameNetworkManager.playerName = File.ReadAllText(testingNamePath);
    }
}
