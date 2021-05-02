using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance localPlayerInstance;
    
    [SyncVar] public string playerName = "null";
    [SyncVar] public GameObject playerEntity;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        Debug.Log("Starting local player instance");
        localPlayerInstance = this;
        ChangeName(GameNetworkManager.playerName);
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
        
        Debug.Log("Spawning local player");
        GameObject player = Entity.Spawn("Player", playerName, new Vector2(0, 80)).gameObject;
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        player.GetComponent<Player>().displayName = playerName;
        player.GetComponent<Player>().playerInstance = gameObject;
        playerEntity = player;
    }
}
