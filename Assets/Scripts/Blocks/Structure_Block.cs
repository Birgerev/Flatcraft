using System;
using UnityEngine;
using Random = System.Random;

public class Structure_Block : Block
{
    public override string texture { get; set; } = "block_structure";
    public override float breakTime { get; } = 10000000;

    public override void GeneratingTick()
    {
        Tick();
        
        base.GeneratingTick();
    }
    
    public override void Tick()
    {
        if (age == 0) //prevent block from being ticked multiple times, since it would create an infinite loop
        {
            base.Tick();

            if (GetData().HasTag("structure"))
            {
                var structureId = GetData().GetTag("structure");
                var structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
                var structure = structures[new Random(
                    SeedGenerator.SeedByLocation(location)).Next(0, structures.Length)];

                BlockState replaceState = new BlockState(Material.Air);

                foreach (var blockText in structure.text.Split('\n', '\r'))
                {
                    if (blockText.Length < 4)
                        continue;
                    
                    var mat = (Material) Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                    Location loc = new Location(
                        int.Parse(blockText.Split('*')[1].Split(',')[0]),
                        int.Parse(blockText.Split('*')[1].Split(',')[1]),
                        location.dimension);
                    BlockData data = new BlockData(blockText.Split('*')[2]);
                    BlockState state = new BlockState(mat, data);

                    if (loc.x == 0 && loc.y == 0)
                    {
                        replaceState = state;
                        continue;
                    }

                    loc += location;

                    
                    loc.SetState(state).Tick();
                }

                location.SetState(replaceState).Tick();
            }
        }
    }
}