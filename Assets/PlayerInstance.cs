using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public PlayerInstance localPlayerInstance;
    
    [SyncVar] public string playerName = "null";
    [SyncVar] public GameObject playerEntity;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        localPlayerInstance = this;
        ChangeName(GameNetworkManager.PlayerName);
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

    [Command]
    public void SpawnPlayerEntity()
    {
        if (playerEntity != null)
            return;
        
        GameObject player = Entity.Spawn("Player").gameObject;
        NetworkServer.Spawn(player);
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        player.GetComponent<Player>().displayName = playerName;
        playerEntity = player;
    }
}
