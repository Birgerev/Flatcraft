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
        if(CreateNameCheck())
            return;
        
        SceneManager.LoadScene("MainMenu");
    }

    private bool CreateNameCheck()
    {
        string testingNamePath = Application.persistentDataPath + "\\testingProfile.dat";
        if(!File.Exists(testingNamePath))
        {
            SceneManager.LoadScene("TestingCreateName");
            return true;
        }
        
        GameNetworkManager.playerName = File.ReadAllText(testingNamePath);
        return false;
    }
}
