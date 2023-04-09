using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Boot : MonoBehaviour
{
    private const string TestWorldName = "Test";
    
    public bool autoloadTestWorld;
    
    void Start()
    {
        //Option to immediately load test world in editor
        if (autoloadTestWorld && Application.isEditor)
        {
            if(World.WorldExists(TestWorldName))
                WorldManager.world = World.LoadWorld(TestWorldName);
            else
                WorldManager.world = new World(TestWorldName, (new System.Random()).Next());
            
            MultiplayerManager.HostGameAsync();
            return;
        }
        
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
        
        //GameNetworkManager.playerName = File.ReadAllText(testingNamePath);
        return false;
    }
}
