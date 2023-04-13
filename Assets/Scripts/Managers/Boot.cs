using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Boot : MonoBehaviour
{
    public bool autoloadTestWorld;
    
    void Start()
    {
        //Option to immediately load test world in editor
        if (autoloadTestWorld && Application.isEditor)
        {
            WorldManager.world = World.LoadWorld("Test");
            MultiplayerManager.HostGameAsync();
            return;
        }
        
        SceneManager.LoadScene("MainMenu");
    }
}
