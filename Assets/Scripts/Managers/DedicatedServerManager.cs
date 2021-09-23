using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DedicatedServerManager
{
    private static string serverConfigPath = Application.dataPath + "/../serverConfig.dat";
    public static Dictionary<string, string> configValues = new Dictionary<string, string>();
    
    public static bool DedicatedServerCheck()
    {
        //TODO ports, database
        if(File.Exists(serverConfigPath))
        {
            string[] lines = File.ReadAllLines(serverConfigPath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                configValues.Add(parts[0], parts[1]);
            }
            
            
            
            //Load or create a world
            World world = new World("world", new System.Random().Next());
            if (World.LoadWorlds().Count > 0)
                world = World.LoadWorlds()[0];
            WorldManager.world = world;
            
            GameNetworkManager.connectionMode = ConnectionMode.DedicatedServer;
            SceneManager.LoadScene("Game");
            return true;
        }
        
        return false;
    }
}
