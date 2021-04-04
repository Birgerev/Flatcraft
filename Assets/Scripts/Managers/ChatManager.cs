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
        if (args[0].Equals("/give"))
        {
            try
            {
                ItemStack item = new ItemStack((Material) Enum.Parse(typeof(Material), args[1]), int.Parse(args[2]));

                player.playerEntity.GetComponent<Player>().GetInventory().AddItem(item);
                ChatAddMessage("Gave " + player.playerName + " " + item.amount + " " + item.material + "'s");
            }
            catch (Exception e)
            {
                ChatAddMessage("/give command failed for item: " + args[1] + ", amount: " + args[2]);
                Debug.LogError("chat error: " + e.StackTrace);
            }
        }
        if (args[0].Equals("/stork"))
        {
            ChatAddMessage("meeeeeee");
        }
    }

    [ClientRpc]
    public void ChatAddMessage(string text)
    {
        ChatMenu.instance.AddMessage(text);
    }
}