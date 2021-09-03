using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Boot : MonoBehaviour
{
    void Start()
    {
        string testingNamePath = Application.dataPath + "/../testingProfile.dat";
        if(!File.Exists(testingNamePath))
        {
            SceneManager.LoadScene("TestingCreateName");
            return;
        }
        GameNetworkManager.playerName = File.ReadAllText(testingNamePath);
        
        SceneManager.LoadScene("MainMenu");
    }
}
