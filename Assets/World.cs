using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class World 
{
    public string name;
    public int seed;
    public float time;

    public World(string name, int seed)
    {
        this.name = name;
        this.seed = seed;
    }

    public World()
    {

    }

    public static World loadWorld(string name)
    {
        World world = new World(name, 0);
        Dictionary<string, string> worldData = new Dictionary<string, string>();
        string[] data = File.ReadAllLines(world.getPath() + "\\level.dat");
        foreach(string dataLine in data)
        {
            worldData.Add(dataLine.Split('=')[0], dataLine.Split('=')[1]);
        }
        
        world.seed = int.Parse(worldData["seed"]);
        world.time = float.Parse(worldData["time"]);

        return world;
    }

    public static bool worldExists(string name)
    {
        return File.Exists(new World(name, 0).getPath() + "\\level.dat");
    }

    public static List<World> loadWorlds()
    {
        List<World> worlds = new List<World>();

        foreach (string worldName in Directory.GetDirectories(GetSavesPath()))
        {
            worlds.Add(loadWorld(worldName.Split('\\')[worldName.Split('\\').Length-1]));
        }

        return worlds;
    }

    public void Delete()
    {
        Directory.Delete(getPath(), true);
    }

    public void SaveData()
    {
        if(!Directory.Exists(getPath()))
            Directory.CreateDirectory(getPath());
        if (!Directory.Exists(getPath() + "\\region"))
            Directory.CreateDirectory(getPath()+"\\region");
        if (!Directory.Exists(getPath() + "\\region\\Overworld"))
            Directory.CreateDirectory(getPath() + "\\region\\Overworld");
        if (!Directory.Exists(getPath() + "\\players"))
            Directory.CreateDirectory(getPath() + "\\players");

        if(!File.Exists(getPath() + "\\level.dat"))
            File.Create(getPath() + "\\level.dat").Close();

        List<string> data = new List<string>();

        data.Add("seed=" + seed);
        data.Add("time=" + time);

        File.WriteAllLines(getPath() + "\\level.dat", data);
    }

    public static string GetSavesPath()
    {
        string savesPath = Application.dataPath + "\\..\\Saves\\";

        if (!Directory.Exists(savesPath))
            Directory.CreateDirectory(savesPath);
        return savesPath;
    }
    public string getPath()
    {

        return GetSavesPath() + name;
    }

    public float getDiskSize()
    {
        return CalculateFolderSize(getPath());
    }

    protected static float CalculateFolderSize(string folder)
    {
        float folderSize = 0.0f;
        //Checks if the path is valid or not
        if (!Directory.Exists(folder))
            return folderSize;
        else
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                if (File.Exists(file))
                {
                    FileInfo finfo = new FileInfo(file);
                    folderSize += finfo.Length;
                }
            }

            foreach (string dir in Directory.GetDirectories(folder))
                folderSize += CalculateFolderSize(dir);
        }
        return folderSize;
    }
}
