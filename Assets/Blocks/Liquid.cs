using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : Block
{
    public override float breakTime { get; } = 100;
    public virtual int max_liquid_level { get; } = 8;

    public string debugData;

    public override void FirstTick()
    {
        base.FirstTick();
    }

    public override void Tick()
    {
        base.Tick();

        //If not covered by a block
        if (!data.ContainsKey("liquid_level"))
            data.Add("liquid_level", max_liquid_level.ToString());

        debugData = stringFromData(data);

        Flow();
        CheckSource();
    }

    public void CheckSource()
    {
        if (data["liquid_level"] == max_liquid_level.ToString())
            return;

        if (!data.ContainsKey("source_block") || !data.ContainsKey("liquid_level"))
        {
            Chunk.setBlock(getPosition(), Material.Air, true);
            return;
        }

        Vector2Int source = new Vector2Int(int.Parse(data["source_block"].Split('.')[0]), (int.Parse(data["source_block"].Split('.')[1])));
        Block sourceBlock = Chunk.getBlock(getPosition() + source);

        if(sourceBlock == null || !sourceBlock.data.ContainsKey("liquid_level"))
        {
            Chunk.setBlock(getPosition(), Material.Air, true);
            return;
        }
        if(sourceBlock.GetMateral() != GetMateral() || 
            int.Parse(sourceBlock.data["liquid_level"]) <= 1 || 
            int.Parse(data["liquid_level"]) < 1 ||
            int.Parse(sourceBlock.data["liquid_level"]) < int.Parse(data["liquid_level"]))
        {
            Chunk.setBlock(getPosition(), Material.Air, true);
        }
    }

    public void Flow()
    {
        if (Chunk.getBlock(getPosition() + new Vector2Int(0, -1)) != null && Chunk.getBlock(getPosition() + new Vector2Int(0, -1)).GetMateral() == GetMateral())
            return;
        if (Chunk.getBlock(getPosition() + new Vector2Int(0, -1)) == null)
        {
            Chunk.setBlock(getPosition() + new Vector2Int(0, -1), GetMateral(), "source_block=0.1", true);
            return;
        }

        int liquidLevel = int.Parse(data["liquid_level"]);

        if (liquidLevel > 1)
        {
            if (Chunk.getBlock(getPosition() + new Vector2Int(-1, 0)) == null)
            {
                Chunk.setBlock(getPosition() + new Vector2Int(-1, 0), GetMateral(), "source_block=1.0,liquid_level="+(liquidLevel-1), true);
            }
            if (Chunk.getBlock(getPosition() + new Vector2Int(1, 0)) == null)
            {
                Chunk.setBlock(getPosition() + new Vector2Int(1, 0), GetMateral(), "source_block=-1.0,liquid_level=" + (liquidLevel - 1), true);
            }
        }
    }

    public override void Hit(float time)
    {

    }

    public override Sprite getTexture()
    {
        if (!data.ContainsKey("liquid_level"))
            return Resources.LoadAll<Sprite>("Sprites/" + texture)[max_liquid_level-1];

        return Resources.LoadAll<Sprite>("Sprites/" + texture)[int.Parse(data["liquid_level"]) - 1];
    }
}
