using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static Dictionary<Vector2Int, int> lightSources = new Dictionary<Vector2Int, int>();
    public static List<Vector2Int> sunlightSources = new List<Vector2Int>();

    public string texture;
    public virtual string[] alternative_textures { get; } = { };
    public virtual float change_texture_time { get; } = 0;
    
    public virtual bool playerCollide { get; } = true;
    public virtual bool trigger { get; } = false;
    public virtual bool requiresGround { get; } = false;
    public virtual bool autosave { get; } = false;
    public virtual bool autoTick { get; } = false;
    public virtual float breakTime { get; } = 0.75f;
    public virtual bool rotate_x { get; } = false;
    public virtual bool rotate_y { get; } = false;

    public virtual Tool_Type propperToolType { get; } = Tool_Type.None;
    public virtual Tool_Level propperToolLevel { get; } = Tool_Level.None;
    
    public virtual int glowLevel { get; } = 0;

    public Dictionary<string, string> data = new Dictionary<string, string>();

    public float blockHealth = 0;

    public int randomTickNumber = 0;
    
    private float time_of_last_hit = 0;
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
        UpdateColliders();

        if (rotate_x || rotate_y)
        {
            Rotate();
        }

        if (autoTick)
            StartCoroutine(autoTickLoop());
    }

    public virtual void GeneratingTick()
    {
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

        UpdateBlockLight();
        if (spread)
        {
            UpdateLightAtSources();
            RenderBlockLight();
        }

        randomTickNumber = new System.Random(Chunk.seedByPosition(getPosition())).Next(0, 1000);

        UpdateColliders();

        RenderRotate();
        
        if (spread)
            SpreadTick(getPosition());
    }

    IEnumerator autoTickLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1 / Chunk.TickRate);
            Tick(false);
        }
    }
    
    public static void SpreadTick(Vector2Int pos)
    {
        List<Block> blocks = new List<Block>();

        blocks.Add(Chunk.getBlock(pos + new Vector2Int(0, 1)));
        blocks.Add(Chunk.getBlock(pos + new Vector2Int(0, -1)));
        blocks.Add(Chunk.getBlock(pos + new Vector2Int(-1, 1)));
        blocks.Add(Chunk.getBlock(pos + new Vector2Int(1, 0)));

        foreach (Block block in blocks) {
            if (block != null)
            {
                block.Tick(false);
            }
        }
    }
    public static void UpdateSunlightSourceList()
    {
        //Clear previous sources
        List<Vector2Int> sources = sunlightSources.ToList();

        sunlightSources.Clear();
        foreach (Vector2Int pos in sunlightSources)
        {
            if(Chunk.getBlock(pos) != null)
                Chunk.getBlock(pos).UpdateBlockLight();
        }

        //Find new sources and populate the list
        int lightRenderDistance = (Chunk.RenderDistance * 12);
        for (int x = (int)Player.localInstance.transform.position.x - lightRenderDistance; x < (int)Player.localInstance.transform.position.x + lightRenderDistance; x++)
        {
            Block topBlock = Chunk.getTopmostBlock(x);
            if (topBlock == null)
                continue;

            int y = topBlock.getPosition().y;

            sunlightSources.Add(new Vector2Int(x, y));
            topBlock.UpdateBlockLight();
        }


    }

    public bool IsSunlightSource()
    {
        return sunlightSources.Contains(getPosition());
    }

    public void UpdateBlockLight()
    {
        if (GetLightSourceLevel(getPosition()) > 0)
            lightSources[getPosition()] = GetLightSourceLevel(getPosition());
    }


    public void RenderBlockLight()
    {
        float brightnessColorValue = (float)GetLightLevel(getPosition()) / 15f;
        GetComponent<SpriteRenderer>().color = new Color(brightnessColorValue, brightnessColorValue, brightnessColorValue);
    }

    public static void UpdateLight()
    {
        if (WorldManager.instance.loadingProgress != 1 && WorldManager.instance.loadingState != "Waiting For Light")
            return;

        //Update Sunlight
        foreach (Chunk chunk in WorldManager.instance.chunks.Values)
        {
            foreach (Block block in chunk.GetComponentsInChildren<Block>())
            {
                block.RenderBlockLight();
            }
        }
    }

    public static void UpdateLightAtSources()
    {
        if (WorldManager.instance.loadingProgress != 1)
            return;
        //Update Sunlight
        UpdateSunlightSourceList();

        //Make Sure List is correct
        foreach (Vector2Int key in new List<Vector2Int>(lightSources.Keys))
        {
            Block block = Chunk.getBlock(key);

            if (block == null || GetLightSourceLevel(block.getPosition()) == 0)
            {
                lightSources.Remove(key);
                continue;
            }

            lightSources[key] = GetLightSourceLevel(block.getPosition());
        }

        List<Vector2Int> sources = new List<Vector2Int>(lightSources.Keys);
        sources.AddRange(sunlightSources);
        List<Vector2Int> updatedBlocks = new List<Vector2Int>();

        //Update all nearby lights
        foreach (Vector2Int key in sources)
        {
            for (int x = -7; x > 7; x++)
            {
                for (int y = -7; y > 7; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    //Prevent a block from being updated more than once, for performance
                    if (updatedBlocks.Contains(pos))
                        continue;
                    updatedBlocks.Add(pos);


                    Block block = Chunk.getBlock(pos);
                    //Skip if block is air
                    if (block == null)
                        continue;

                    //Update light if everything checks out
                    block.RenderBlockLight();
                }
            }
        }
    }

    public static int GetLightSourceLevel(Vector2Int pos)
    {
        Block block = Chunk.getBlock(pos);
        if (block == null)
            return 0;
        
        return block.IsSunlightSource() ? 15 : block.glowLevel;
    }

    public static int GetLightLevel(Vector2Int pos)
    {
        if (lightSources.Count <= 0)
            return 0;

        Vector2Int brightestSourcePos = Vector2Int.zero;
        int brightestValue = 0;
        int i = 0;
        foreach(Vector2Int source in lightSources.Keys)
        {
            int value = GetLightSourceLevel(source) - (int)(Vector2Int.Distance(source, pos));

            if(value > brightestValue)
            {
                brightestValue = value;
                brightestSourcePos = source;
            }
            i++;
        }

        return brightestValue;
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
        Chunk.setBlock(getPosition(), GetMaterial(), stringFromData(data), true, false);
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