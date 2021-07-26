public class Diamond_Shovel : Shovel
{
    public override string texture { get; set; } = "item_diamond_shovel";
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
}