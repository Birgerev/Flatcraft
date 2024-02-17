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
        if (CreateNameCheck()) return;
        if (TryLoadTestWorld()) return;
        
        SceneManager.LoadScene("MainMenu");//Load main menu first, so it is active
        SceneManager.LoadScene("Intro", LoadSceneMode.Additive);
    }

    private bool CreateNameCheck()
    {
        string testingNamePath = Application.persistentDataPath + "\\testingProfile.dat";
        if(!File.Exists(testingNamePath))
        {
            SceneManager.LoadScene("TestingCreateName");
            return true;
        }
        
        SettingsManager.PlayerName = File.ReadAllText(testingNamePath);
        return false;
    }

    private bool TryLoadTestWorld()
    {
        if (autoloadTestWorld && Application.isEditor)
        {
            if(World.WorldExists(TestWorldName))
                WorldManager.world = World.LoadWorld(TestWorldName);
            else
                WorldManager.world = new World(TestWorldName, (new System.Random()).Next());
            
            MultiplayerManager.HostGameAsync();
            return true;
        }

        return false;
    }
}
