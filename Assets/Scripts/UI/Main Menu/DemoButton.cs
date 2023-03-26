using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoButton : MonoBehaviour
{
    private const string demoWorldName = "demo";
    
    public void Click()
    {
        List<World> worlds = World.LoadWorlds();
        bool createWorld = true;
        World demoWorld = null;

        //If demo world exists, load it instead
        foreach (World world in worlds)
        {
            if (world.name.Equals(demoWorldName))
            {
                createWorld = false;
                demoWorld = world;
                break;
            }
        }
        
        //If demo world doesn't exist, create demo world
        if (createWorld)
        {
            demoWorld = new World(demoWorldName, new System.Random().Next());
            demoWorld.template = WorldTemplate.Skyblock;
            
            //Save world
            demoWorld.SaveData();
        }
        
        //Load world
        WorldManager.world = demoWorld;
        MultiplayerManager.HostGameAsync();
        LoadingMenu.Create(LoadingMenuType.LoadWorld);
    }
}
