﻿public class Tool : Item
{
    public virtual Tool_Type tool_type { get; } = Tool_Type.None;
    public virtual Tool_Level tool_level { get; } = Tool_Level.None;

    public override void InteractLeft(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        var block = loc.GetBlock();

        if (block != null) 
            block.Hit(player, 1 / Player.interactionsPerPerSecond, tool_type, tool_level);
    }

    public override void InteractRight(PlayerInstance player, Location loc, bool firstFrameDown)
    {
        //TODO move these interactions into block.Interact()
        if (tool_type == Tool_Type.Hoe && (loc.GetMaterial() == Material.Grass || loc.GetMaterial() == Material.Dirt))
        {
            loc.SetMaterial(Material.Farmland_Dry).Tick();
            Player.localEntity.DoToolDurability();
        }

        if (tool_type == Tool_Type.FlintAndSteel && loc.GetMaterial() == Material.Air)
        {
            loc.SetMaterial(Material.Fire).Tick();
            Player.localEntity.DoToolDurability();
            Sound.Play(loc, "random/flint_and_steel/click", SoundType.Entities, 0.8f, 1.2f);
        }

        if (tool_type == Tool_Type.FlintAndSteel && loc.GetMaterial() == Material.TNT)
        {
            ((TNT)loc.GetBlock()).Prime();
            Player.localEntity.DoToolDurability();
            Sound.Play(loc, "random/flint_and_steel/click", SoundType.Entities, 0.8f, 1.2f);
        }

        base.InteractRight(player, loc, firstFrameDown);
    }
}

public enum Tool_Type
{
    None,
    Pickaxe,
    Axe,
    Shovel,
    Hoe,
    Sword,
    FlintAndSteel,
}

public enum Tool_Level
{
    None = 1,
    Wooden = 2,
    Stone = 3,
    Gold = 4,
    Iron = 5,
    Diamond = 6
}