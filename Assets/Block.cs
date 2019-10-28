using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public string texture;
    public virtual string[] alternative_textures { get; } = { };
    public virtual float change_texture_time { get; } = 0;

    public virtual bool playerCollide { get; } = true;
    public virtual bool trigger { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual bool autosave { get; } = false;
    public virtual float breakTime { get; } = 0.75f;
    public virtual bool rotate_x { get; } = false;
    public virtual bool rotate_y { get; } = false;

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
        UpdateColliders();

        if (rotate_x || rotate_y)
        {
            Rotate();
        }
    }

    public virtual void GeneratingTick()
    {
        firstTick = false;
    }

    public virtual void Tick(bool spread)
    {
        if (Time.time - time_of_last_hit > 1.5f)
        {
            ResetBlockDamage();
        }

        if (requiresGround)
        {
            if(Chunk.getBlock(getPosition() - new Vector2Int(0, 1)) == null)
            {
                Break();
            }
        }

        UpdateLight();

        randomTickNumber = new System.Random(Chunk.seedByPosition(getPosition())).Next(0, 1000);

        UpdateColliders();

        RenderRotate();

        if (spread)
            SpreadTick();
    }

    public void SpreadTick()
    {
        List<Block> blocks = new List<Block>();

        blocks.Add(Chunk.getBlock(new Vector2Int(0, 1)));
        blocks.Add(Chunk.getBlock(new Vector2Int(0, -1)));
        blocks.Add(Chunk.getBlock(new Vector2Int(-1, 1)));
        blocks.Add(Chunk.getBlock(new Vector2Int(1, 0)));

        foreach (Block block in blocks) {
            if (block != null)
            {
                block.Tick(false);
            }
        }
    }
    
    public void UpdateLight()
    {
        /*
        if (glowingLevel != 0)
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
        }*/
    }

    public virtual void UpdateColliders()
    {
        GetComponent<Collider2D>().enabled = (playerCollide || trigger);
        GetComponent<Collider2D>().isTrigger = (trigger);
    }

    public void Rotate()
    {
        bool rotated_x = false;
        bool rotated_y = false;

        if (rotate_y)
        {
            rotated_y = (Player.localInstance.transform.position.y < getPosition().y);
        }
        if (rotate_x)
        {
            rotated_x = (Player.localInstance.transform.position.x < getPosition().x);
        }

        data["rotated_x"] = rotated_x ? "true" : "false";
        data["rotated_y"] = rotated_y ? "true" : "false";
    }

    public void RenderRotate()
    {
        bool rotated_x = false;
        bool rotated_y = false;

        if (data.ContainsKey("rotated_x"))
            rotated_x = (data["rotated_x"] == "true");
        if (data.ContainsKey("rotated_y"))
            rotated_y = (data["rotated_y"] == "true");

        GetComponent<SpriteRenderer>().flipX = rotated_x;
        GetComponent<SpriteRenderer>().flipY = rotated_y;
    }



    public virtual void Autosave()
    {
        time_of_last_autosave = Time.time;

        Chunk.setBlock(getPosition(), GetMaterial(), stringFromData(data), true);
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
            time *= 2 + ((float)tool_level * 2f);
        }
        if (propperToolLevel == Tool_Level.None ||
            (tool_type == propperToolType && tool_level >= propperToolLevel))
        {
            properToolStats = true;
        }

        blockHealth -= time;


        RenderBlockDamage();

        Tick(true);

        if (blockHealth <= 0)
        {
            if (properToolStats)
                Break();
            else
                Break(false);
        }
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
        return new ItemStack(GetMaterial(), 1);
    }

    public virtual void Interact()
    {
        Tick(true);
    }

    public void ResetBlockDamage()
    {
        blockHealth = breakTime;
        RenderBlockDamage();
    }

    public virtual void RenderBlockDamage()
    {
        Transform damageIndicator = transform.Find("BreakIndicator");

        if (blockHealth != breakTime)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
            int spriteIndex = (int)((blockHealth/breakTime) / (1f / ((float)sprites.Length)));

            BreakIndicator.instance.UpdateState(spriteIndex, getPosition());
        }
    }

    public virtual void Render()
    {
        GetComponent<SpriteRenderer>().sprite = getTexture();
    }

    public virtual Sprite getTexture()
    {
        if (change_texture_time > 0 && alternative_textures.Length > 0)
        {
            float totalTimePerTextureLoop = change_texture_time * alternative_textures.Length;
            int textureIndex = (int)((Time.time % totalTimePerTextureLoop) / change_texture_time);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }
        else if(alternative_textures.Length > 0)
        {
            int textureIndex = new System.Random(Chunk.seedByPosition(getPosition())).Next(0, alternative_textures.Length);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }
        else return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMaterial()
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