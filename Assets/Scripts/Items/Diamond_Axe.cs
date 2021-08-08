﻿public class Diamond_Axe : Axe
{
    public override string texture { get; set; } = "item_diamond_axe";
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurability { get; } = 1561;
}