using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance localPlayerInstance;

    [SyncVar] public ulong steamId = 0;
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
        ChangeSteamId(SteamUser.GetSteamID().m_SteamID);
        RequestJoinMessage();
    }

    [Command]
    public void ChangeSteamId(ulong newSteamId)
    {
        steamId = newSteamId;
    }

    public override void OnStopServer()
    {
        ChatManager.instance.AddMessage(GetPlayerName() + " left the world");

        base.OnStopServer();
    }

    [Command]
    public void RequestJoinMessage()
    {
        if (ChatManager.instance != null)
            ChatManager.instance.AddMessage(GetPlayerName() + " joined the world");
    }

    [Command]
    public void SpawnPlayerEntity()
    {
        if (playerEntity != null)
            return;

        Debug.Log("Spawning local player");
        GameObject player = Entity.Spawn("Player", steamId.ToString(), new Vector2(0, 80)).gameObject;
        player.GetComponent<Player>().displayName = GetPlayerName();
        player.GetComponent<Player>().playerInstance = gameObject;
        playerEntity = player;
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    public string GetPlayerName()
    {
        return SteamFriends.GetFriendPersonaName((CSteamID)steamId);
    }
}