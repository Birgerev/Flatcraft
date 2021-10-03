using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DedicatedServer
{
    public int id;
    public string name;
    public string description;
    public string address;
    public int port;
    public string owner;
    public int playerCount;
    public int maxPlayerCount;

    public DedicatedServer(int id, string name, string description, string address, int port, string owner, int playerCount, int maxPlayerCount)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.address = address;
        this.port = port;
        this.owner = owner;
        this.playerCount = playerCount;
        this.maxPlayerCount = maxPlayerCount;
    }
    
    public DedicatedServer(string dataText)
    {
        string[] parts = dataText.Split('*');
        
        this.id = int.Parse(parts[0]);
        this.name = parts[1];
        this.description = parts[2];
        this.address = parts[3];
        this.port = int.Parse(parts[4]);
        this.owner = parts[5];
        this.playerCount = int.Parse(parts[6]);
        this.maxPlayerCount = int.Parse(parts[7]);
    }
}
