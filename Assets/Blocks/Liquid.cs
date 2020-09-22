using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : Block
{
    public override float breakTime { get; } = 100;
    public virtual int max_liquid_level { get; } = 8;
    public override bool triggerCollider { get; } = true;
    public override bool autoTick { get; } = true;

    private Location left;
    private Location right;
    private Location top;
    private Location bottom;

    public override void Tick()
    {
        if (!data.ContainsKey("liquid_level"))
            data.Add("liquid_level", max_liquid_level.ToString());

        if (age == 0)
        {
            left = (location + new Location(-1, 0));
            right = (location + new Location(1, 0));
            top = (location + new Location(0, 1));
            bottom = (location + new Location(0, -1));
        }

        //if (age > 1 && (left.GetMaterial() == Material.Air || right.GetMaterial() == Material.Air || top.GetMaterial() == Material.Air || bottom.GetMaterial() == Material.Air))
        //    Flow();
        //CheckSource();
        
        base.Tick();
    }

    public void CheckSource()
    {
        if (data["liquid_level"] == max_liquid_level.ToString())
            return;

        if (!data.ContainsKey("source_block") || !data.ContainsKey("liquid_level"))
        {
            location.SetMaterial(Material.Air);
            return;
        }

        Location source = new Location(int.Parse(data["source_block"].Split('.')[0]), (int.Parse(data["source_block"].Split('.')[1])));
        Location sourceLocation = (location + source);

        if(sourceLocation.GetMaterial() == Material.Air || dataFromString(sourceLocation.GetData()).ContainsKey("liquid_level"))
        {
            location.SetMaterial(Material.Air);
            return;
        }
        if(sourceLocation.GetMaterial() != GetMaterial() || 
           int.Parse(dataFromString(sourceLocation.GetData())["liquid_level"]) <= 1 || 
           int.Parse(data["liquid_level"]) < 1 ||
           int.Parse(dataFromString(sourceLocation.GetData())["liquid_level"]) < int.Parse(data["liquid_level"]))
        {
            location.SetMaterial(Material.Air);
        }
    }

    public void Flow()
    {
        if (bottom.GetMaterial() == GetMaterial())
            return;
        
        if (bottom.GetMaterial() == Material.Air)
        {
           (location + new Location(0, -1)).SetMaterial(GetMaterial()).SetData("source_block=0.1");
            return;
        }

        int liquidLevel = int.Parse(data["liquid_level"]);

        if (liquidLevel > 1)
        {
            if (left.GetMaterial() == Material.Air)
            {
                (location + new Location(-1, 0)).SetMaterial(GetMaterial()).SetData("source_block=1.0,liquid_level="+(liquidLevel-1));
            }
            if (right.GetMaterial() == Material.Air)
            {
                (location + new Location(1, 0)).SetMaterial(GetMaterial()).SetData("source_block=1.0,liquid_level="+(liquidLevel-1));
            }
        }
    }

    public override void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {

    }

    public override Sprite getTexture()
    {
        if (!data.ContainsKey("liquid_level"))
            return Resources.LoadAll<Sprite>("Sprites/" + texture)[max_liquid_level-1];

        return Resources.LoadAll<Sprite>("Sprites/" + texture)[int.Parse(data["liquid_level"]) - 1];
    }

    public virtual void OnTriggerEnter2D(Collider2D col)
    {
        if(col.GetComponent<Entity>() != null && !col.GetComponent<Entity>().isInLiquid)
        {
            col.GetComponent<Entity>().EnterLiquid(this);
        }
    }

    public virtual void OnTriggerStay2D(Collider2D col)
    {
        if(col.GetComponent<Entity>() != null)
        {
            col.GetComponent<Entity>().isInLiquid = true;
        }
    }
    
    public virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null && col.GetComponent<Entity>().isInLiquid)
        {
            col.GetComponent<Entity>().ExitLiquid(this);
        }
    }
}
