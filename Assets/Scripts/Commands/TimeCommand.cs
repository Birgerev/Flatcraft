using System;

public class TimeCommand : Command
{
    public TimeCommand()
    {
        name = "time";
        parameters = new string[] {"<Time>"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            if (param.Length != 1)
            {
                ChatManager.instance.AddMessage("Usage: " + UsageGuide());
                return;
            }

            int time = int.Parse(param[0]);

            WorldManager.world.time = time;
            ChatManager.instance.AddMessage("Set time to " + time);
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}