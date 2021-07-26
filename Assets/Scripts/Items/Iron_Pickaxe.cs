public class Iron_Pickaxe : Pickaxe
{
    public override string texture { get; set; } = "item_iron_pickaxe";
    public override Tool_Level tool_level { get; } = Tool_Level.Iron;
    public override int maxDurabulity { get; } = 250;
}