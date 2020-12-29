public class Water : Liquid
{
    public override string texture { get; set; } = "block_water";
    public override int max_liquid_level { get; } = 8;
    
    public override void LiquidEncounterFlow(Location relativeLocation)
    {
        Location loc = location + relativeLocation;
        
        if (((Liquid) loc.GetBlock()).IsLiquidSourceBlock())    //If water flows down to a lava source, make it to obsidian
        {
            loc.SetMaterial(Material.Obsidian).Tick();
        }
        else                                                    //Water flowing onto a non source lava block, make it cobblestone
        {
            loc.SetMaterial(Material.Cobblestone).Tick();
        }
        
        base.LiquidEncounterFlow(relativeLocation);
    }
}