using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public static Dictionary<string, string> Values = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        
        LoadSettings();
        
        InvokeRepeating(nameof(SaveSettings), 5f, 5f);
    }

    private void LoadSettings()
    {
        if(!File.Exists(GetPath()))
            return;
        
        string[] lines = File.ReadAllLines(GetPath());

        foreach (var line in lines)
        {
            string key = line.Split('=')[0];
            string value = line.Split('=')[1];

            Values[key] = value;
        }
    }

    private void SaveSettings()
    {
        List<String> lines = new List<string>();
        
        foreach (string keyName in Values.Keys)
        {
            string value = Values[keyName];
            
            lines.Add(keyName + "=" + value);
        }
        
        File.WriteAllLines(GetPath(), lines);
    }
    
    private void SaveSettings()
    {
        List<String> lines = new List<string>();
        
        foreach (string keyName in Values.Keys)
        {
            string value = Values[keyName];
            
            lines.Add(keyName + "=" + value);
        }
        
        File.WriteAllLines(GetPath(), lines);
    }
    
    public string GetPath()
    {
        return Application.persistentDataPath + "\\options.txt";
    }
}
