using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class World
{
    public static string cachedAppPath = "";

    public string name;
    public int seed;
    public float time;
    public int versionId;

    public World(string name, int seed)
    {
        this.name = name;
        this.seed = seed;
    }

    public World()
    {
    }

    public static string appPath
    {
        get
        {
            if (cachedAppPath != "")
                return cachedAppPath;

            cachedAppPath = Application.dataPath;
            return cachedAppPath;
        }
    }

    public static World loadWorld(string name)
    {
        var world = new World(name, 0);
        var worldData = new Dictionary<string, string>();
        var data = File.ReadAllLines(world.getPath() + "\\level.dat");
        foreach (var dataLine in data) 
            worldData.Add(dataLine.Split('=')[0], dataLine.Split('=')[1]);

        world.seed = int.Parse(worldData["seed"]);
        world.time = float.Parse(worldData["time"]);
        world.versionId = int.Parse(worldData["versionId"]);

        return world;
    }

    public static bool worldExists(string name)
    {
        return File.Exists(new World(name, 0).getPath() + "\\level.dat");
    }

    public static List<World> loadWorlds()
    {
        var worlds = new List<World>();

        foreach (var worldName in Directory.GetDirectories(GetSavesPath()))
            worlds.Add(loadWorld(worldName.Split('\\')[worldName.Split('\\').Length - 1]));

        return worlds;
    }

    public void Delete()
    {
        Directory.Delete(getPath(), true);
    }

    public void SaveData()
    {
        if (!Directory.Exists(getPath()))
            Directory.CreateDirectory(getPath());
        if (!Directory.Exists(getPath() + "\\chunks"))
            Directory.CreateDirectory(getPath() + "\\chunks");
        if (!Directory.Exists(getPath() + "\\chunks\\Overworld"))
            Directory.CreateDirectory(getPath() + "\\chunks\\Overworld");
        if (!Directory.Exists(getPath() + "\\chunks"))
            Directory.CreateDirectory(getPath() + "\\players");
        if (!Directory.Exists(getPath() + "\\inventories"))
            Directory.CreateDirectory(getPath() + "\\inventories");

        if (!File.Exists(getPath() + "\\level.dat"))
            File.Create(getPath() + "\\level.dat").Close();

        var data = new List<string>();

        data.Add("seed=" + seed);
        data.Add("time=" + time);
        data.Add("versionId=" + versionId);

        File.WriteAllLines(getPath() + "\\level.dat", data);
    }

    public static string GetSavesPath()
    {
        var savesPath = appPath + "\\..\\Saves\\";

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
        var folderSize = 0.0f;
        //Checks if the path is valid or not
        if (!Directory.Exists(folder))
        {
            return folderSize;
        }

        foreach (var file in Directory.GetFiles(folder))
            if (File.Exists(file))
            {
                var finfo = new FileInfo(file);
                folderSize += finfo.Length;
            }

        foreach (var dir in Directory.GetDirectories(folder))
            folderSize += CalculateFolderSize(dir);
        return folderSize;
    }
}