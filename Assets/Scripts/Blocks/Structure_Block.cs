using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Structure_Block : Block
{
    public override float breakTime { get; } = 10000000;
    private bool hasBeenTicked = false;

    public override void GeneratingTick()
    {
        Tick();

        base.GeneratingTick();
    }

    public override void Tick()
    {
        if (!hasBeenTicked) //prevent block from being ticked multiple times, since it would create an infinite loop
        {
            hasBeenTicked = true;

            if (GetData().HasTag("structure"))
            {
                string structureId = GetData().GetTag("structure");
                TextAsset[] structures = Resources.LoadAll<TextAsset>("Structure/" + structureId);
                if (structures.Length == 0)
                {
                    Debug.LogError("Error in loading structure '" + structureId + "', doesn't exist");
                    return;
                }
                TextAsset structure =
                    structures[new Random(SeedGenerator.SeedByWorldLocation(location)).Next(0, structures.Length)];

                BlockState replaceState = new BlockState(Material.Air);
                List<Location> locationsToTick = new List<Location>();
                

                foreach (string blockText in structure.text.Split(Environment.NewLine.ToCharArray()))
                {
                    try
                    {
                        if (blockText.Length < 4)
                            continue;

                        Material mat = (Material) Enum.Parse(typeof(Material), blockText.Split('*')[0]);
                        Location loc = new Location(int.Parse(blockText.Split('*')[1].Split(',')[0]),
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


                        loc.SetState(state);
                        locationsToTick.Add(loc);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error in loading structure for structure block, corrupted line: '" + blockText + "'.   " + e.Message + e.StackTrace);
                    }
                }

                location.SetState(replaceState).Tick();
                
                //Tick all blocks in structure
                foreach (var loc in locationsToTick)
                {
                    //Location.Tick() spreads, which only serves to worsen performance
                    Block block = loc.GetBlock();
                    
                    if(block != null)
                        block.Tick();
                }
            }
        }
        
        base.Tick();
    }
}