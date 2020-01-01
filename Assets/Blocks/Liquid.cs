using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : Block
{
    public override float breakTime { get; } = 100;
    public virtual int max_liquid_level { get; } = 8;
    public override bool trigger { get; } = true;
    public override bool autoTick { get; } = true;

    public string debugData;

    private int age;

    private Block leftBlock;
    private Block rightBlock;
    private Block topBlock;
    private Block bottomBlock;

    public override void Tick(bool spread)
    {
        //If not covered by a block
        if (!data.ContainsKey("liquid_level"))
            data.Add("liquid_level", max_liquid_level.ToString());

        debugData = stringFromData(data);

        if (leftBlock == null)
            leftBlock = Chunk.getBlock(position + new Vector2Int(-1, 0));
        if (rightBlock == null)
            rightBlock = Chunk.getBlock(position + new Vector2Int(1, 0));
        if (topBlock == null)
            topBlock = Chunk.getBlock(position + new Vector2Int(0, 1));
        if (bottomBlock == null)
            bottomBlock = Chunk.getBlock(position + new Vector2Int(0, -1));

        if (age > 1 && (leftBlock == null || rightBlock == null || topBlock == null || bottomBlock == null))
            Flow();
        CheckSource();

        age++;

        base.Tick(spread);
    }

    public void CheckSource()
    {
        if (data["liquid_level"] == max_liquid_level.ToString())
            return;

        if (!data.ContainsKey("source_block") || !data.ContainsKey("liquid_level"))
        {
            Chunk.setBlock(position, Material.Air, "", true, false);
            return;
        }

        Vector2Int source = new Vector2Int(int.Parse(data["source_block"].Split('.')[0]), (int.Parse(data["source_block"].Split('.')[1])));
        Block sourceBlock = Chunk.getBlock(position + source);

        if(sourceBlock == null || !sourceBlock.data.ContainsKey("liquid_level"))
        {
            Chunk.setBlock(position, Material.Air, "", true, false);
            return;
        }
        if(sourceBlock.GetMaterial() != GetMaterial() || 
            int.Parse(sourceBlock.data["liquid_level"]) <= 1 || 
            int.Parse(data["liquid_level"]) < 1 ||
            int.Parse(sourceBlock.data["liquid_level"]) < int.Parse(data["liquid_level"]))
        {
            Chunk.setBlock(position, Material.Air, "", true, false);
        }
    }

    public void Flow()
    {
        if (bottomBlock != null && bottomBlock.GetMaterial() == GetMaterial())
            return;
        if (bottomBlock == null)
        {
            Chunk.setBlock(position + new Vector2Int(0, -1), GetMaterial(), "source_block=0.1", true, true);
            return;
        }

        int liquidLevel = int.Parse(data["liquid_level"]);

        if (liquidLevel > 1)
        {
            if (leftBlock == null)
            {
                Chunk.setBlock(position + new Vector2Int(-1, 0), GetMaterial(), "source_block=1.0,liquid_level="+(liquidLevel-1), true, true);
            }
            if (rightBlock == null)
            {
                Chunk.setBlock(position + new Vector2Int(1, 0), GetMaterial(), "source_block=-1.0,liquid_level=" + (liquidLevel - 1), true, true);
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

    public virtual void OnTriggerStay2D(Collider2D col)
    {
        if(col.GetComponent<Entity>() != null)
        {
            col.GetComponent<Entity>().isInLiquid = true;
        }
    }

    public virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<Entity>() != null)
        {
            col.GetComponent<Entity>().isInLiquid = false;
        }
    }
}
