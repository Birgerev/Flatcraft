using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class PlayerInstance : NetworkBehaviour
{
    public static PlayerInstance localPlayerInstance;

    [SyncVar] public ulong uuid = 0;
    [SyncVar] public Player playerEntity;

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
        
#if !DISABLESTEAMWORKS
        ChangeSteamId(SteamUser.GetSteamID().m_SteamID);
#endif
        
        RequestJoinMessage();
    }

    [Command]
    public void ChangeSteamId(ulong newSteamId)
    {
        uuid = newSteamId;
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
        GameObject player = Entity.Spawn("Player", uuid.ToString(), new Vector2(0, 80)).gameObject;
        player.GetComponent<Player>().displayName = GetPlayerName();
        player.GetComponent<Player>().playerInstance = this;
        playerEntity = player.GetComponent<Player>();
        player.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    public string GetPlayerName()
    {
#if !DISABLESTEAMWORKS
        return SteamFriends.GetFriendPersonaName((CSteamID)steamId);
#endif
        
        return "Steve";
    }
}