using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager instance;

    public void Start()
    {
        instance = this;
    }

    [Command(requiresAuthority = false)]
    public void RequestSendMessage(string text, NetworkConnectionToClient sender = null)
    {
        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        if (text.StartsWith("/"))
        {
            ExcecuteCommand(text.Split(' '), player);
            return;
        }

        ChatAddMessage("<" + player.playerName + "> " + text);
    }

    [Server]
    public void ExcecuteCommand(string[] args, PlayerInstance player)
    {
        ChatAddMessage("cool command " + args[0]);
    }

    [ClientRpc]
    public void ChatAddMessage(string text)
    {
        ChatMenu.instance.AddMessage(text);
    }
}
