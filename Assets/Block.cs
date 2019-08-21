using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string texture;
    public virtual bool playerCollide { get; } = true;
    public virtual bool requiresGround { get; } = false;
    public virtual float breakTime { get; } = 0.75f;

    public virtual Tool_Type propperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level propperToolLevel { get; } = Tool_Level.None;



    public Dictionary<string, string> data = new Dictionary<string, string>();

    public float blockHealth = 0;

    public int randomTickNumber = 0;

    private bool firstTick = true;

    private void Start()
    {
        gameObject.name = "block [" + transform.position.x + "," + transform.position.y + "]";
        blockHealth = breakTime;

        texture = (string)this.GetType().
            GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        Render();
    }
    public virtual void FirstTick()
    {
        firstTick = false;
    }

    public virtual void Tick()
    {
        if (firstTick)
            FirstTick();

        if (requiresGround)
        {
            if(Chunk.getBlock(getPosition() - new Vector2Int(0, 1)) == null)
            {
                Break();
            }
        }

        randomTickNumber = new System.Random().Next(0, 1000);

        GetComponent<Collider2D>().enabled = (playerCollide);
    }

    public virtual void Hit(float time)
    {
        Hit(time, Tool_Type.None, Tool_Level.None);
    }

    public virtual void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {
        bool properToolStats = false;

        print(tool_type + " == "+ propperToolType +" | "+tool_level +" == "+ propperToolLevel);
        if(tool_type == propperToolType && tool_level >= propperToolLevel)
        {
            time *= 1 + ((float)tool_level * 2f);
        }
        if (tool_level == Tool_Level.None ||
            (tool_type == propperToolType && tool_level >= propperToolLevel))
        {
            properToolStats = true;
        }

        blockHealth -= time;

        if (blockHealth <= 0)
        {
            if(properToolStats)
                Break();
            else
                Break(false);
        }

        RenderBlockDamage();
    }

    public virtual void Break()
    {
        Break(true);
    }

    public virtual void Break(bool drop)
    {
        if(drop)
            Drop();

        Chunk.setBlock(getPosition(), Material.Air);
    }

    public virtual void Drop()
    {
        GetDrop().Drop(getPosition());
    }

    public virtual ItemStack GetDrop()
    {
        return new ItemStack(GetMateral(), 1);
    }

    public virtual void Interact()
    {
    }

    public virtual void RenderBlockDamage()
    {
        Transform damageIndicator = transform.Find("BreakIndicator");

        if(blockHealth == breakTime)
        {
            if(damageIndicator != null)
            {
                Destroy(damageIndicator);
            }
        }
        else
        {
            if (damageIndicator == null)
            {
                damageIndicator = new GameObject("BreakIndicator").transform;
                damageIndicator.transform.SetParent(transform);
                damageIndicator.transform.localPosition = Vector3.zero;
                damageIndicator.gameObject.AddComponent<SpriteRenderer>();
            }
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
            int spriteIndex = (int)((blockHealth/breakTime) / (1f / ((float)sprites.Length)));


            damageIndicator.GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
        }
    }

    public virtual void Render()
    {
        GetComponent<SpriteRenderer>().sprite = getTexture();
    }

    public virtual Sprite getTexture()
    {
        return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMateral()
    {
        return (Material)System.Enum.Parse(typeof(Material), this.GetType().Name);
    }

    public Vector2Int getPosition()
    {
        return Vector2Int.CeilToInt((Vector2)transform.position);
    }

    public static Dictionary<string, string> dataFromString(string dataString)
    {
        Dictionary<string, string> resultData = new Dictionary<string, string>();

        foreach (string dataPiece in dataString.Split(','))
        {
            if(dataPiece.Contains("="))
                resultData.Add(dataPiece.Split('=')[0], dataPiece.Split('=')[1]);
        }

        return resultData;
    }

    public static string stringFromData(Dictionary<string, string> data)
    {
        string result = "";

        bool first = true;
        foreach (string key in data.Keys)
        {
            foreach (string value in data.Values)
            {
                if (!first)
                    result += ",";
                result += key + "=" + value; 
                first = false;
            }
        }

        return result;
    }
}