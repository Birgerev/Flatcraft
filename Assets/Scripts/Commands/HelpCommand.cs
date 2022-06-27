using System;

public class HelpCommand : Command
{
    public HelpCommand()
    {
        name = "help";
        parameters = new string[] {""};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            ChatManager.instance.AddMessage("---- Commands ----");
            foreach (Command cmd in ChatManager.instance.commands)
            {
                string line = "/" + cmd.name;
                foreach (string parameter in cmd.parameters)
                {
                    line = line + " " + parameter;
                }
                
                ChatManager.instance.AddMessage(line);
            }
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}