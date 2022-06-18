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
            Debug.Log("A server config was found, starting a dedicated server");
            Debug.Log("Reading config file");
            ReadInConfigValues();
            if (!DoesMandatoryConfigValueExist("port") || !DoesMandatoryConfigValueExist("worldName"))
                return false;
            
            Debug.Log("Config read was succesful");
            
            //Load or create a world
            World world = new World(configValues["worldName"], new System.Random().Next());

            if (World.WorldExists(configValues["worldName"]))
                world = World.LoadWorld(configValues["worldName"]);
            WorldManager.world = world;
            Debug.Log("Loaded server world '" + configValues["worldName"] + "'");
            
            //Start server
            GameNetworkManager.port = int.Parse(configValues["port"]);
            GameNetworkManager.connectionMode = ConnectionMode.DedicatedServer;
            SceneManager.LoadScene("Game");
            return true;
        }
        
        return false;
    }

    private static void ReadInConfigValues()
    {
        string[] lines = File.ReadAllLines(serverConfigPath);
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            configValues.Add(parts[0], parts[1]);
        }
    }
    
    private static bool DoesMandatoryConfigValueExist(string key)
    {
        if (configValues.ContainsKey(key))
            return true;
        
        Debug.LogError("Server config file '" + serverConfigPath + "' does not specify a '" + key + "' value, can't start dedicated server");
        return false;
    }
}
