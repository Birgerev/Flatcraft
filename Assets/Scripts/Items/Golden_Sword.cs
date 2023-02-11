public class Golden_Sword : Sword
{
    public override Tool_Level tool_level { get; } = Tool_Level.Gold;
    public override int maxDurability { get; } = 32;
    public override float entityDamage { get; } = 4;
}