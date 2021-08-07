using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : Block
{
    public override float breakTime { get; } = 100;
    public virtual int maxLiquidLevel { get; } = 8;
    public virtual float liquidTickFrequency { get; } = 1;
    public override bool solid { get; set; } = false;
    public override bool trigger { get; set; } = true;

    public override ItemStack GetDrop()
    {
        return new ItemStack();
    }

    public override void BuildTick()
    {
        //If block was built by player, it is a liquid source
        MakeIntoLiquidSourceBlock();

        base.BuildTick();
    }

    public override void GeneratingTick()
    {
        MakeIntoLiquidSourceBlock();
        Tick();

        base.GeneratingTick();
    }

    public override void Tick()
    {
        if (!GetData().HasTag("liquid_level"))
        {
            SetData(GetData().SetTag("liquid_level", maxLiquidLevel.ToString()));
            location.Tick();
            return;
        }
        
        StartCoroutine(scheduleLiquidTick());

        base.Tick();
    }
    
    public virtual void LiquidTick()
    {
        if (CheckSource())
            CheckFlow();
    }
    
    private IEnumerator scheduleLiquidTick()
    {
        yield return new WaitForSeconds(1 / Chunk.TickRate * liquidTickFrequency);
        LiquidTick();
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

        if (TryFlow(new Location(0, -1), maxLiquidLevel))
            return;

        if (downMat == GetMaterial())
            return;

        int liquidLevel = int.Parse(GetData().GetTag("liquid_level"));

        if (liquidLevel > 1)
        {
            bool flowLeft = true;
            bool flowRight = true;

            for (int x = 1; x < liquidLevel; x++)
            {
                Location leftIteration = location + new Location(-x, 0);
                Location rightIteration = location + new Location(x, 0);

                if (CanFlowToLocation(leftIteration) && !CanFlowToLocation(rightIteration))
                {
                    flowRight = false;
                    break;
                }

                if (CanFlowToLocation(rightIteration) && !CanFlowToLocation(leftIteration))
                {
                    flowLeft = false;
                    break;
                }
            }

            if (flowLeft)
                TryFlow(new Location(-1, 0), liquidLevel - 1);

            if (flowRight)
                TryFlow(new Location(1, 0), liquidLevel - 1);
        }
    }
    
    private bool CanFlowToLocation(Location loc)
    {
        Location locBelow = loc + new Location(0, -1);
        if (IsLocationSolid(loc) || IsLocationSolid(locBelow))
            return false;

        return true;
    }
    
    private bool IsLocationSolid(Location loc)
    {
        Block block = loc.GetBlock();

        if (block != null)
        {
            if(block.solid)
                return true;

            //Prevent flowing upstream
            if (block.GetMaterial() == GetMaterial())
            {
                int currentLevel = int.Parse(GetData().GetTag("liquid_level"));
                int locLevel = int.Parse(block.GetData().GetTag("liquid_level"));
                
                if(locLevel >= currentLevel - 1)
                    return true;
            }
        }
        

        return false;
    }
    
    public bool TryFlow(Location relativeLocation, int liquidLevel)
    {
        Location loc = location + relativeLocation;

        if (!IsLocationSolid(loc)) //Flow if no block is in the way
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
        BlockData newData = GetData();
        newData.SetTag("source_block", "true");
        newData.SetTag("liquid_level", maxLiquidLevel.ToString());
        SetData(newData);
        location.Tick();
    }

    protected virtual void LiquidEncounterEffect(Location loc)
    {
        Sound.Play(loc, "block/fire/break", SoundType.Blocks, 0.8f, 1.2f);
        Particle.Spawn_SmallSmoke(loc.GetPosition(), Color.black);
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
                    int possibleSourceLiquidLevel = int.Parse(possibleSourceData.GetTag("liquid_level"));
                    int currentLiquidLevel = int.Parse(GetData().GetTag("liquid_level"));

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

    public override void Hit(PlayerInstance player, float time, Tool_Type tool_type, Tool_Level tool_level)
    {
    }

    public override Sprite getTexture()
    {
        if (!GetData().HasTag("liquid_level"))
            return Resources.LoadAll<Sprite>("Sprites/" + texture)[maxLiquidLevel - 1];

        return Resources.LoadAll<Sprite>("Sprites/" + texture)[int.Parse(GetData().GetTag("liquid_level")) - 1];
    }
}