using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSaveData
{
    public static string GetPath()
    {
        return WorldManager.world.GetPath() + "\\players";
    }

    public static string GetPlayerPath(string Player)
    {
        return GetPath() + "\\" + Player + "\\playerData.dat";
    }

    public static void SetBedLocation(string player, Location loc)
    {
        EditLine(player, "bedLocation", JsonUtility.ToJson(loc));
    }

    public static Location GetBedLocation(string player)
    {
        string locString = ReadLine(player, "bedLocation");

        if (locString == null)
            return new Location();

        return JsonUtility.FromJson<Location>(locString);
    }

    public static void EditLine(string player, string key, string value)
    {
        if (!File.Exists(GetPlayerPath(player)))
            File.Create(GetPlayerPath(player)).Close();

        List<string> lines = new List<string>(ReadFile(player));
        bool lineWasChanged = false;
        for (int i = 0; i < lines.Count; i++)
            if (lines[i].Split('=')[0].Equals(key))
            {
                lines[i] = key + "=" + value;
                lineWasChanged = true;
                break;
            }

        if (!lineWasChanged)
            lines.Add(key + "=" + value);

        File.WriteAllLines(GetPlayerPath(player), lines);
    }

    public static string ReadLine(string player, string key)
    {
        if (!File.Exists(GetPlayerPath(player)))
            return null;

        List<string> lines = new List<string>(ReadFile(player));
        for (int i = 0; i < lines.Count; i++)
            if (lines[i].Split('=')[0].Equals(key))
                return lines[i].Split('=')[1];

        return null;
    }

    public static string[] ReadFile(string player)
    {
        string[] lines = new string[0];

        if (File.Exists(GetPlayerPath(player)))
            lines = File.ReadAllLines(GetPlayerPath(player));

        return lines;
    }
}