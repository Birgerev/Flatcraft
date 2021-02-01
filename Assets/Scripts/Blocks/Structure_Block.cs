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

            if (data.HasData("structure"))
            {
                var structureId = data.GetData("structure");
                var structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
                var structure =
                    structures[new Random(SeedGenerator.SeedByLocation(location)).Next(0, structures.Length)];

                var replaceMaterial = Material.Air;
                var replaceData = new BlockData();

                foreach (var blockText in structure.text.Split('\n', '\r'))
                {
                    if (blockText.Length < 4)
                        continue;
                    var mat = (Material) Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                    var loc = new Location(
                        int.Parse(blockText.Split('*')[1].Split(',')[0]),
                        int.Parse(blockText.Split('*')[1].Split(',')[1]),
                        location.dimension);
                    var data = new BlockData(blockText.Split('*')[2]);

                    if (loc.x == 0 && loc.y == 0)
                    {
                        replaceMaterial = mat;
                        replaceData = data;
                        continue;
                    }

                    loc += location;

                    loc.SetMaterial(mat).SetData(data);
                }

                location.SetMaterial(replaceMaterial).SetData(replaceData);
            }
        }
    }
}