using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string texture;
    public virtual bool playerCollide { get; } = true;
    public virtual bool trigger { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual bool autosave { get; } = false;
    public virtual float breakTime { get; } = 0.75f;

    public virtual Tool_Type propperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level propperToolLevel { get; } = Tool_Level.None;
    
    public virtual int glowingLevel { get; } = 0;
    public virtual float flickerLevel { get; } = 0;
    public virtual Color glowingColor { get; } = new Color(1, 0.75f, 0.4f);

    public Dictionary<string, string> data = new Dictionary<string, string>();

    public float blockHealth = 0;

    public int randomTickNumber = 0;

    private bool firstTick = true;
    private float time_of_last_hit = 0;
    private float time_of_last_autosave = 0;
    private void Start()
    {
        gameObject.name = "block [" + transform.position.x + "," + transform.position.y + "]";
        blockHealth = breakTime;
        
        texture = (string)this.GetType().
            GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);

        FirstTick();
        Render();
    }

    public virtual void FirstTick()
    {
    }

    public virtual void GeneratingTick()
    {
        firstTick = false;
    }

    public virtual void Tick()
    {
        if (requiresGround)
        {
            if(Chunk.getBlock(getPosition() - new Vector2Int(0, 1)) == null)
            {
                Break();
            }
        }

        if (Time.time - time_of_last_hit > 0.5f)
        {
            ResetBlockDamage();
        }
        if(glowingLevel != 0)
        {
            GameObject light;
            if (transform.Find("_light"))
            {
                light = transform.Find("_light").gameObject;
            }
            else
            {
                light = Instantiate((GameObject)Resources.Load("Objects/BlockLight"));
                light.transform.SetParent(transform);
                light.transform.localPosition = Vector3.zero;
                light.transform.name = "_light";
            }
            
            light.GetComponent<BlockLight>().glowingLevel = glowingLevel;
            light.GetComponent<BlockLight>().color = glowingColor;
            light.GetComponent<BlockLight>().flickerLevel = flickerLevel;
        }

        randomTickNumber = new System.Random().Next(0, 1000);

        GetComponent<Collider2D>().enabled = (playerCollide || trigger);
        GetComponent<Collider2D>().isTrigger = (trigger);

        if (Time.time - time_of_last_autosave > Chunk.AutosaveDuration && autosave)
        {
            Autosave();
            return;
        }
    }

    public virtual void Autosave()
    {
        time_of_last_autosave = Time.time;

        Chunk.setBlock(getPosition(), GetMateral(), stringFromData(data), true);
    }

    public virtual void Hit(float time)
    {
        Hit(time, Tool_Type.None, Tool_Level.None);
    }

    public virtual void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {
        time_of_last_hit = Time.time;

        bool properToolStats = false;

        if(tool_type == propperToolType && tool_level >= propperToolLevel)
        {
            time *= 1 + ((float)tool_level * 2f);
        }
        if (propperToolLevel == Tool_Level.None ||
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

    public void ResetBlockDamage()
    {
        blockHealth = breakTime;
        RenderBlockDamage();
    }

    public virtual void RenderBlockDamage()
    {
        Transform damageIndicator = transform.Find("BreakIndicator");

        if(blockHealth == breakTime)
        {
            if(damageIndicator != null)
            {
                Destroy(damageIndicator.gameObject);
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

        foreach (string dataPiece in dataString.Split('|'))
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
                    result += "|";
                result += key + "=" + value; 
                first = false;
            }
        }

        return result;
    }
}