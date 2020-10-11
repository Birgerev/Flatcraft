using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Liquid : Block
{
    public override float breakTime { get; } = 100;
    public virtual int max_liquid_level { get; } = 8;
    public virtual float liquidTickFrequency { get; } = 1;
    public override bool triggerCollider { get; } = true;
    
    public override void BuildTick()
    {
        //If block was built by player, it is a liquid source
        MakeIntoLiquidSourceBlock();
        
        base.BuildTick();
    }

    public override void GeneratingTick()
    {
        MakeIntoLiquidSourceBlock();
        
        base.GeneratingTick();
    }
    
    public void ScheduleLiquidTick()
    {
        StartCoroutine(scheduleLiquidTick());
    }

    private IEnumerator scheduleLiquidTick()
    {
        yield return new WaitForSeconds((1 / Chunk.TickRate) * liquidTickFrequency);
        Tick();
    }

    public override void Tick()
    {
        if (!data.HasData("liquid_level"))
            data.SetData("liquid_level", max_liquid_level.ToString());

        if(CheckSource())
            Flow();
        
        base.Tick();
    }

    public bool CheckSource()
    {
        if (IsLiquidSourceBlock())
            return true;

        var source = FindSourceBlock();

        if (source.Equals(new Location(0, 0)))
        {
            foreach (var sourceResult in FindSourceResultBlocks())    //Tick all neighboring liquids
            {
                Liquid liquid = (Liquid)sourceResult.GetBlock();
                liquid.ScheduleLiquidTick();
            }
            
            location.SetMaterial(Material.Air);
            
            return false;
        }

        return true;
    }

    public void Flow()
    {
        Location down = (location + new Location(0, -1));
        Material downMat = (location + new Location(0, -1)).GetMaterial();
        
        if (downMat == GetMaterial())
            return;
        
        if (downMat == Material.Air)
        {
            down.SetMaterial(GetMaterial()).SetData(new BlockData("liquid_level=" + max_liquid_level.ToString()));
            ((Liquid)down.GetBlock()).ScheduleLiquidTick();
            return;
        }
        
        var liquidLevel = int.Parse(data.GetData("liquid_level"));
        
        if (liquidLevel > 1)
        {
            Location left = location + new Location(-1, 0);
            Location right = location + new Location(1, 0);
            var flowLeft = true;
            var flowRight = true;
            
            for (int x = 1; x < liquidLevel; x++)
            {
                Location leftIteration = location + new Location(-x, 0);
                Location leftDownIteration = leftIteration + new Location(0, -1);
                Location rightIteration = location + new Location(x, 0);
                Location rightDownIteration = rightIteration + new Location(0, -1);

                if ((leftIteration.GetMaterial() == Material.Air && leftDownIteration.GetMaterial() == Material.Air) &&
                    !(rightIteration.GetMaterial() == Material.Air && rightDownIteration.GetMaterial() == Material.Air))
                {
                    flowRight = false;
                    break;
                }
                
                if ((rightIteration.GetMaterial() == Material.Air && rightDownIteration.GetMaterial() == Material.Air) &&
                    !(leftIteration.GetMaterial() == Material.Air && leftDownIteration.GetMaterial() == Material.Air))
                {
                    flowLeft = false;
                    break;
                }
            }

            if (flowLeft && left.GetMaterial() == Material.Air)
            {
                left.SetMaterial(GetMaterial()).SetData(new BlockData("liquid_level=" + (liquidLevel - 1)));
                ((Liquid)left.GetBlock()).ScheduleLiquidTick();
            }
            if (flowRight && right.GetMaterial() == Material.Air)
            {
                right.SetMaterial(GetMaterial()).SetData(new BlockData("liquid_level=" + (liquidLevel - 1)));
                ((Liquid)right.GetBlock()).ScheduleLiquidTick();
            }
        }
    }
    public bool IsLiquidSourceBlock()
    {
        return data.GetData("source_block") == "true";
    }
    
    public void MakeIntoLiquidSourceBlock()
    {
        data.SetData("source_block", "true");
        Autosave();
    }
    
    public Location FindSourceBlock()
    {
        Location up = location + new Location(0, 1);    //Vertical source check
        if (up.GetMaterial() == GetMaterial())
        {
            return up;
        }
        
        
        //Horizontal source checks
        List<Location> possibleHorizontalBlocks = new List<Location>() { location + new Location(-1, 0), location + new Location(1, 0)};
        
        foreach (var possibleSource in possibleHorizontalBlocks)        
        {
            var possibleSourceMaterial = possibleSource.GetMaterial();
            var currentMaterial = GetMaterial();
            
            if (possibleSourceMaterial == currentMaterial)     //Make sure the source is of the same material, otherwise lava and water could use eachother as sources)
            {
                var possibleSourceData = possibleSource.GetData();
                if (possibleSourceData.HasData("liquid_level"))
                {
                    var possibleSourceLiquidLevel = int.Parse(possibleSourceData.GetData("liquid_level"));
                    var currentLiquidLevel = int.Parse(data.GetData("liquid_level"));

                    if (possibleSourceLiquidLevel > currentLiquidLevel         //Make sure the source is of a higher liquid level
                        && possibleSourceLiquidLevel >= 1)                     //If the source liquid level is less than 1, it's not a viable source
                    {
                        return possibleSource;
                    }
                }
            }
        }
        

        return new Location(0, 0);        //return null
    }
    
    public List<Location> FindSourceResultBlocks()
    {
        List<Location> sourceResults = new List<Location>();
        List<Location> possibleSourceBlocks = new List<Location>() { location + new Location(-1, 0), location + new Location(1, 0), location + new Location(0, -1)};
        
        foreach (var possibleSource in possibleSourceBlocks)
        {
            var possibleSourceMaterial = possibleSource.GetMaterial();
            var currentMaterial = GetMaterial();
            
            if (possibleSourceMaterial == currentMaterial)     //Make sure the source is of the same material, otherwise lava and water could use eachother as sources)
            {
                var possibleSourceData = possibleSource.GetData();
                if (possibleSourceData.HasData("liquid_level"))
                {
                    var possibleSourceLiquidLevel = int.Parse(possibleSourceData.GetData("liquid_level"));
                    var currentLiquidLevel = int.Parse(data.GetData("liquid_level"));

                    if (possibleSourceLiquidLevel < currentLiquidLevel)         //Make sure the source is of a lower liquid level
                    {
                        sourceResults.Add(possibleSource);
                    }
                }
            }
        }

        return sourceResults;        //return list
    }

    public override void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {
    }

    public override Sprite getTexture()
    {
        if (!data.HasData("liquid_level"))
            return Resources.LoadAll<Sprite>("Sprites/" + texture)[max_liquid_level - 1];

        return Resources.LoadAll<Sprite>("Sprites/" + texture)[int.Parse(data.GetData("liquid_level")) - 1];
    }

    public virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null && !col.GetComponent<Entity>().isInLiquid)
            col.GetComponent<Entity>().EnterLiquid(this);
    }

    public virtual void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null) col.GetComponent<Entity>().isInLiquid = true;
    }

    public virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null && col.GetComponent<Entity>().isInLiquid)
            col.GetComponent<Entity>().ExitLiquid(this);
    }
}