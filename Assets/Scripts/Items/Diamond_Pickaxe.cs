public class Diamond_Pickaxe : Pickaxe
{
    public override string texture { get; set; } = "item_diamond_pickaxe";
    public override Tool_Level tool_level { get; } = Tool_Level.Diamond;
    public override int maxDurabulity { get; } = 1561;
}