using Mirror;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance localPlayerInstance;

    [SyncVar] public string playerName = "unassigned name";
    [SyncVar] public GameObject playerEntity;

    public void Update()
    {
        if (!isLocalPlayer || !NetworkClient.ready)
            return;

        if (playerEntity == null && !DeathMenu.active)
            SpawnPlayerEntity();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Debug.Log("Starting local player instance");
        localPlayerInstance = this;
        //TODO name ChangeName(GameNetworkManager.playerName);
        RequestJoinMessage();
    }

    [Command]
    public void ChangeName(string name)
    {
        playerName = name;
    }

    public override void OnStopServer()
    {
        ChatManager.instance.AddMessage(playerName + " left the world");

        base.OnStopServer();
    }

    [Command]
    public void RequestJoinMessage()
    {
        if (ChatManager.instance != null)
            ChatManager.instance.AddMessage(playerName + " joined the world");
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