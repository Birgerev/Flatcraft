using System;

public class StructureCommand : Command
{
    public StructureCommand()
    {
        name = "structure";
        parameters = new string[] {"<structure>"};
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

            Player playerEntity = player.playerEntity.GetComponent<Player>();
            Location playerLoc = playerEntity.Location;
            string structureName = param[0];

            playerLoc.SetState(new BlockState(
                Material.Structure_Block,
                new BlockData("structure=" + structureName))).Tick();

            ChatManager.instance.AddMessage("Structure block created");
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}