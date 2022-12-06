
using System;

public class HealCommand : Command
{
    public HealCommand()
    {
        name = "heal";
        parameters = new string[] {"[amount]"};
    }

    public override void Execute(string[] param, PlayerInstance player)
    {
        base.Execute(param, player);
        
        try
        {
            Player playerEntity = player.playerEntity.GetComponent<Player>();
            float amount = playerEntity.maxHealth - playerEntity.health;
            if (param.Length >= 1)
                amount = int.Parse(param[0]);

            playerEntity.health += amount;
            ChatManager.instance.AddMessage("Healed " + player.playerName);
        }
        catch (Exception e){ SendCommandError(e.StackTrace); }
    }
}
