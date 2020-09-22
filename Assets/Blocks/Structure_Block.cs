using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure_Block : Block
{
    public static string default_texture  = "block_structure";
    public override float breakTime { get; } = 10000000;

    public override void Tick()
    {
        if (age == 0)   //prevent block from being ticked multiple times, since it would create an infinite loop
        {
            base.Tick();
            
            if (data.ContainsKey("structure"))
            {
                string structureId = data["structure"];
                TextAsset[] structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
                TextAsset structure = structures[new System.Random(SeedGenerator.SeedByLocation(location)).Next(0, structures.Length)];
                
                Material replaceMaterial = Material.Air;
                string replaceData = "";

                foreach (string blockText in structure.text.Split(new char[] { '\n', '\r' }))
                {
                    if (blockText.Length < 4)
                        continue;
                    Material mat = (Material)System.Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                    Location loc = new Location(
                        int.Parse(blockText.Split('*')[1].Split(',')[0]),
                        int.Parse(blockText.Split('*')[1].Split(',')[1]),
                        location.dimension);
                    string data = blockText.Split('*')[2];

                    if (loc.x == 0 && loc.y == 0)
                    {
                        replaceMaterial = mat;
                        replaceData = data;
                        continue;
                    }

                    loc += location;

                    loc.SetMaterial(mat);
                }
                location.SetMaterial(replaceMaterial).SetData(replaceData);
            }
        }
    }
}
