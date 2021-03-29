using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance localPlayerInstance;
    
    [SyncVar] public string playerName = "null";
    [SyncVar] public GameObject playerEntity;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        localPlayerInstance = this;
        ChangeName(GameNetworkManager.PlayerName);
        RequestJoinMessage();
    }
    public void Update()
    {
        if (!isLocalPlayer || ClientScene.readyConnection == null)
            return;
        
        if (playerEntity == null && !DeathMenu.active)
        {
            SpawnPlayerEntity();
        }
    }

    [Command]
    public void ChangeName(string name)
    {
        playerName = name;
    }

    public override void OnStopServer()
    {
        ChatManager.instance.ChatAddMessage(playerName + " left the world");
            
        base.OnStopServer();
    }

    [Command]
    public void RequestJoinMessage()
    {
        if(ChatManager.instance != null)
            ChatManager.instance.ChatAddMessage(playerName + " joined the world");
    }

    [Command]
    public void SpawnPlayerEntity()
    {
        if (playerEntity != null)
            return;
        
        GameObject player = Entity.Spawn("Player").gameObject;
        NetworkServer.Spawn(player);
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        player.GetComponent<Player>().displayName = playerName;
        player.GetComponent<Player>().playerInstance = gameObject;
        playerEntity = player;
    }
}
