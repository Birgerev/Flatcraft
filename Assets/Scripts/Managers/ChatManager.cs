using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager instance;

    public readonly Command[] commands = {new GiveCommand(), new HealCommand(), new HelpCommand(),
        new SaveStructureCommand(), new SummonCommand(), new TimeCommand(), new WeatherCommand()};

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

        AddMessage("<" + player.GetPlayerName() + "> " + text);
    }

    [Server]
    public void ExcecuteCommand(string[] args, PlayerInstance player)
    {
        Debug.Log("executing command> " + args[0]);
        string[] param = args.Skip(1).ToArray();

        foreach (Command cmd in commands)
        {
            if (cmd.name.Equals(args[0].Replace("/", "").ToLower()))
            {
                cmd.Execute(param, player);
            }
        }
    }

    [ClientRpc]
    public void AddMessage(string text)
    {
        Debug.Log("Chat> " + text);
        ChatMenu.instance.AddMessage(text);
    }

    [ClientRpc]
    public void AddMessagePlayer(string text, PlayerInstance player)
    {
        if (PlayerInstance.localPlayerInstance == player)
        {
            Debug.Log("Chat> " + text);
            ChatMenu.instance.AddMessage(text);
        }
    }
}