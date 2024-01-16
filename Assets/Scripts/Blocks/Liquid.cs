using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Liquid : Block
{
    public override float BreakTime { get; } = 100;
    public virtual int maxLiquidLevel { get; } = 8;
    public virtual float liquidTickFrequency { get; } = 1;
    public virtual string[] liquidTextures { get; } = { };
    public override bool IsSolid { get; set; } = false;
    public override bool CanBeOverriden { get; set; } = true;

    private bool _isLiquidTicking;

    protected override ItemStack[] GetDrops()
    {
        return null;
    }

    public override void GeneratingTick()
    {
        MakeIntoLiquidSourceBlock();    //Max liquid level & source block
        
        base.GeneratingTick();
    }

    public override void Tick()
    {
        if (!_isLiquidTicking)
            LiquidTick();

        base.Tick();
    }
    
    public virtual async void LiquidTick()
    {
        _isLiquidTicking = true;
        
        //Make sure block is initialized properly
        if (!GetData().HasTag("liquid_level"))
        {
            MakeIntoLiquidSourceBlock();
            return;
        }
        
        //Delay liquid tick according to defined interval + random offset for performance
        float tickRate = (1f / Chunk.TickRate * liquidTickFrequency);
        //float randomOffset = (float) new System.Random(SeedGenerator.SeedByWorldLocation(location)).NextDouble()*.5f - .25f;
        //Convert to milliseconds
        await Task.Delay((int)(tickRate * 1000));
        
        if (CheckSource())
            CheckFlow();
        
        _isLiquidTicking = false;
    }
    
    public bool CheckSource()
    {
        if (IsLiquidSourceBlock())
            return true;

        Location source = FindSourceBlock();

        if (source.Equals(new Location(0, 0)))
        {
            foreach (Location sourceResult in FindSourceResultBlocks()) //Tick all neighboring liquids
            {
                sourceResult.Tick();
            }

            location.SetMaterial(Material.Air);

            return false;
        }

        return true;
    }

    public void CheckFlow()
    {
        Location down = location + new Location(0, -1);
        Material downMat = down.GetMaterial();

        if (TryFlow(down, maxLiquidLevel))
            return;

        if (downMat == GetMaterial())
            return;

        int liquidLevel = int.Parse(GetData().GetTag("liquid_level"));

        if (liquidLevel > 1)
        {
            Location left = location + new Location(-1, 0);
            Location right = location + new Location(1, 0);
            bool flowLeft = true;
            bool flowRight = true;
            
            //Check for directly adjacent obstacles
            if (!CanFlow(left, true))
            {
                flowLeft = false;
            }
            if (!CanFlow(right, true))
            {
                flowRight = false;
            }
            
            //Check for holes to target if necessary (if direction is contested)
            //Does not account for other liquids in the way, since it would lead to weird "bouncing" of liquids
            if (CanFlow(left, false) && CanFlow(right, false))
            {
                //Ultimate goal is to find hole, so we can target it specifically
                //If we encounter an obstacle, stop searching for holes in that direction
                //We only target a direction when a hole is found
                
                bool searchHolesLeft = true;
                bool searchHolesRight = true;
                
                //Iterate outwards until we find a hole or run out of liquid
                for (int x = 1; x < liquidLevel; x++)
                {
                    bool leftIterationEmpty = CanFlow(location + new Location(-x, 0), false);
                    bool leftIterationHoleEmpty = CanFlow(location + new Location(-x, -1), false);
                    bool rightIterationEmpty = CanFlow(location + new Location(x, 0), false);
                    bool rightIterationHoleEmpty = CanFlow(location + new Location(x, -1), false);

                    
                    //Check for holes on both sides
                    if (searchHolesLeft && searchHolesRight)
                    {
                        if (leftIterationEmpty && leftIterationHoleEmpty && rightIterationEmpty &&
                            rightIterationHoleEmpty)
                        {
                            //If holes on both sides, dont target any direction
                            break;
                        }
                    }
                    //Check for hole on left
                    if (searchHolesLeft)
                    {
                        //If obstacle, stop looking for holes in this direction
                        if (!leftIterationEmpty)
                            searchHolesLeft = false;

                        //If we find a hole, target it and stop looking for more holes
                        if (leftIterationEmpty && leftIterationHoleEmpty)
                        {
                            flowRight = false;
                            break;
                        }
                    }
                    //Check for hole on right
                    if (searchHolesRight)
                    {
                        //If obstacle, stop looking for holes in this direction
                        if (!rightIterationEmpty)
                            searchHolesRight = false;

                        //If we find a hole, target it and stop looking for more holes
                        if (rightIterationEmpty && rightIterationHoleEmpty)
                        {
                            flowLeft = false;
                            break;
                        }
                    }
                }
            }
            
            if (flowLeft)
                TryFlow(left, liquidLevel - 1);

            if (flowRight)
                TryFlow(right, liquidLevel - 1);
        }
    }
    
    private bool CanFlow(Location loc, bool accountLiquidLevel)
    {
        BlockState state = loc.GetState();

        //Can always flow to air
        if (state.material == Material.Air)
        {
            return true;
        }
            
        if (state.material == GetMaterial())
        {
            //Allow filling liquids with less level
            if (accountLiquidLevel && state.data.HasTag("liquid_level"))
            {
                int currentLevel = int.Parse(GetData().GetTag("liquid_level"));
                int locLevel = int.Parse(loc.GetData().GetTag("liquid_level"));

                //locLevel must be two less than current level
                if (currentLevel - locLevel >= 2)
                    return true;
            }//If we do not care about liquid level, we can flow to water
            else return true;
        }
        
        //Otherwise, we cant flow to block
        return false;
    }
    
    public bool TryFlow(Location loc, int liquidLevel)
    {
        if (CanFlow(loc, true)) //Flow if no block is in the way
        {
            BlockState newState = new BlockState(GetMaterial(), new BlockData("liquid_level=" + liquidLevel));

            loc.SetState(newState).Tick();

            return true;
        }

        return false;
    }

    public bool IsLiquidSourceBlock()
    {
        return GetData().GetTag("source_block") == "true";
    }

    private void MakeIntoLiquidSourceBlock()
    {
        SetData(GetData()
            .SetTag("source_block", "true")
            .SetTag("liquid_level", maxLiquidLevel.ToString()))
            .GetBlock().Tick();//Tick without spread
    }

    protected virtual void LiquidEncounterEffect(Location loc)
    {
        Sound.Play(loc, "block/fire/break", SoundType.Block, 0.8f, 1.2f);
        Particle.ClientSpawnSmallSmoke(loc.GetPosition(), Color.black);
    }

    protected Location FindSourceBlock()
    {
        Location up = location + new Location(0, 1); //Vertical source check
        if (up.GetMaterial() == GetMaterial())
            return up;


        //Horizontal source checks
        List<Location> possibleHorizontalBlocks = new List<Location>
            {location + new Location(-1, 0), location + new Location(1, 0)};

        foreach (Location possibleSource in possibleHorizontalBlocks)
        {
            Material possibleSourceMaterial = possibleSource.GetMaterial();
            Material currentMaterial = GetMaterial();

            if (possibleSourceMaterial == currentMaterial
            ) //Make sure the source is of the same material, otherwise lava and water could use eachother as sources)
            {
                BlockData possibleSourceData = possibleSource.GetData();
                if (possibleSourceData.HasTag("liquid_level"))
                {
                    int.TryParse(possibleSourceData.GetTag("liquid_level"), out int possibleSourceLiquidLevel);
                    int.TryParse(GetData().GetTag("liquid_level"), out int currentLiquidLevel);

                    //Make sure the source is of a higher liquid level
                    ////If the source liquid level is less than 1, it's not a viable source
                    if (possibleSourceLiquidLevel > currentLiquidLevel && possibleSourceLiquidLevel >= 1) 
                        return possibleSource;
                }
            }
        }


        return new Location(0, 0); //return null
    }

    protected List<Location> FindSourceResultBlocks()
    {
        List<Location> sourceResults = new List<Location>();
        List<Location> possibleSourceBlocks = new List<Location>
            {location + new Location(-1, 0), location + new Location(1, 0), location + new Location(0, -1)};

        foreach (Location possibleSource in possibleSourceBlocks)
        {
            BlockState possibleSourceState = possibleSource.GetState();
            Material currentMaterial = GetMaterial();

            if (possibleSourceState.material == currentMaterial
            ) //Make sure the source is of the same material, otherwise lava and water could use eachother as sources)
                if (possibleSourceState.data.HasTag("liquid_level"))
                {
                    int possibleSourceLiquidLevel = int.Parse(possibleSourceState.data.GetTag("liquid_level"));
                    int currentLiquidLevel = int.Parse(GetData().GetTag("liquid_level"));
                    
                    //Make sure the source is of a lower liquid level, or a full block (result of liquid flowing down)
                    if (possibleSourceLiquidLevel < currentLiquidLevel || possibleSourceLiquidLevel == maxLiquidLevel)
                        sourceResults.Add(possibleSource);
                }
        }

        return sourceResults; //return list
    }

    public override void Hit(PlayerInstance player, float time, Tool_Type toolType = Tool_Type.None, Tool_Level toolLevel = Tool_Level.None)
    {
        //Disable breaking
    }

    protected override string GetTextureName()
    {
        if (!GetData().HasTag("liquid_level"))
            return base.GetTextureName();

        return liquidTextures[int.Parse(GetData().GetTag("liquid_level")) - 1];
    }
}