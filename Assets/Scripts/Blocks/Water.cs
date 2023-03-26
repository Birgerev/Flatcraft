using UnityEngine;

public class Water : Liquid
{
    public override string[] liquidTextures { get; } =
    {
        "water", "water", 
        "water_1", "water_1", 
        "water_2", "water_2", 
        "water_3", "water_3"
    };
    public override int maxLiquidLevel { get; } = 8;

    public override void Tick()
    {
        CheckLavaEncounter();

        base.Tick();
    }
    
    private void CheckLavaEncounter()
    {
        Location[] relativeDirections = {new Location(0, 1), new Location(0, -1), new Location(-1, 0), new Location(1, 0)};

        foreach (Location relativeDirection in relativeDirections)
        {
            Location loc = location + relativeDirection;

            if (loc.GetMaterial() != Material.Lava)
                continue;
            
            //If lava lands on water, create regular stone
            if (relativeDirection.y == 1) 
            {
                LiquidEncounterEffect(loc);
                location.SetMaterial(Material.Stone).Tick();
                continue;
            }
            
            //If water flows into to a lava source block, turn it in to obsidian
            if (((Liquid) loc.GetBlock()).IsLiquidSourceBlock()) 
            {
                LiquidEncounterEffect(loc);
                loc.SetMaterial(Material.Obsidian).Tick();
                continue;
            }
            
            //if water flows into a non source lava block, make it cobblestone
            LiquidEncounterEffect(loc);
            loc.SetMaterial(Material.Cobblestone).Tick();
        }
    }
}