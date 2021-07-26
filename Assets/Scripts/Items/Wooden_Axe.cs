public class Wooden_Axe : Axe
{
    public override string texture { get; set; } = "item_wooden_axe";
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurabulity { get; } = 59;
}