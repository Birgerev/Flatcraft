using System;

public class SummonCommand : Command
{
    public SummonCommand()
    {
        name = "summon";
        parameters = new string[] {"<Entity Type>", "[amount]"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            string entityType = "";
            int amount = 1;
            if (param.Length >= 1)
                entityType = param[0];
            if (param.Length >= 2)
                amount = int.Parse(param[1]);

            for (int i = 0; i < amount; i++)
            {
                Entity entity = Entity.Spawn(entityType);
                entity.Teleport(player.playerEntity.Location);
                ChatManager.instance.AddMessage("Summoned " + entityType);
            }
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
