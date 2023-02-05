using System;

public class GiveCommand : Command
{
    public GiveCommand()
    {
        name = "give";
        parameters = new string[] {"<material>", "[amount]"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            Material mat = Material.Air;
            int amount = 1;
            if (param.Length >= 1)
                mat = (Material) Enum.Parse(typeof(Material), param[0]);
            if (param.Length >= 2)
                amount = int.Parse(param[1]);
            
            ItemStack item = new ItemStack(mat, amount);

            player.playerEntity.GetComponent<Player>().GetInventory().AddItem(item);
            ChatManager.instance.AddMessage("Gave " + player.GetPlayerName() + " " + item.Amount + " " + item.material + "'s");
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
