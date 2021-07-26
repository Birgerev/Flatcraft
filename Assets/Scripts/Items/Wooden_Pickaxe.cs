public class Wooden_Pickaxe : Pickaxe
{
    public override string texture { get; set; } = "item_wooden_pickaxe";
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurabulity { get; } = 59;
}