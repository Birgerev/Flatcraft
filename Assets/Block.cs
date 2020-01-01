using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public class Block : MonoBehaviour
{
    public static Dictionary<Vector2Int, int> lightSources = new Dictionary<Vector2Int, int>();
    public static HashSet<Vector2Int> sunlightSources = new HashSet<Vector2Int>();

    public static HashSet<Vector2Int> oldLight = new HashSet<Vector2Int>();

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
    public Vector2Int position;
    
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
        
        RenderNoLight();

        //Cache position for use in multithreading
        position = Vector2Int.RoundToInt(transform.position);

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
            if(Chunk.getBlock(position - new Vector2Int(0, 1)) == null)
            {
                Break();
            }
        }

        randomTickNumber = new System.Random(Chunk.seedByPosition(position)).Next(0, 1000);

        UpdateColliders();

        RenderRotate();
        
        if (spread)
            SpreadTick(position);
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
    
    public static void UpdateSunlightSourceAt(int x)
    {
        Block topBlock = Chunk.getTopmostBlock(x);
        if (topBlock == null)
            return;

        //remove all sunlight sources in the same column
        foreach(Vector2Int sourcePos in sunlightSources.ToList())
        {
            if (sourcePos.x == x)
            {
                sunlightSources.Remove(sourcePos);
                UpdateLightAround(sourcePos);
            }
        }

        //Add the new position
        Vector2Int pos = topBlock.position;

        sunlightSources.Add(pos);
        UpdateLightAround(pos);
    }

    public bool IsSunlightSource()
    {
        return sunlightSources.Contains(position);
    }

    public bool CheckBlockLightSource()
    {
        if (glowLevel > 0)
        {
            lightSources[position] = glowLevel;
        }

        return GetLightSourceLevel(position) > 0;
    }
    
    public void RenderNoLight()
    {
        float brightnessColorValue = 0;
        GetComponent<SpriteRenderer>().color = new Color(brightnessColorValue, brightnessColorValue, brightnessColorValue);
    }

    public void RenderBlockLight(int lightLevel)
    {
        float brightnessColorValue = (float)lightLevel / 15f;
        GetComponent<SpriteRenderer>().color = new Color(brightnessColorValue, brightnessColorValue, brightnessColorValue);
    }

    public static void UpdateLightAround(Vector2Int pos)
    {
        Block source = Chunk.getBlock(pos);
        if(source != null)
        {
            source.CheckBlockLightSource();
        }

        for (int x = pos.x - 15; x < pos.x + 15; x++)
        {
            for (int y = pos.y - 15; y < pos.y + 15; y++)
            {
                if (y < 0 || y > Chunk.Height)
                    continue;
                Vector2Int block = new Vector2Int(x, y);
                if (!oldLight.Contains(block))
                    oldLight.Add(block);
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
        if (lightSources.Count <= 0 && sunlightSources.Count <= 0)
            return 0;

        List<Vector2Int> sources = new List<Vector2Int>(lightSources.Keys);
        sources.AddRange(sunlightSources);

        Vector2Int brightestSourcePos = Vector2Int.zero;
        int brightestValue = 0;
        int i = 0;
        foreach(Vector2Int source in sources)
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
            rotated_y = (Player.localInstance.transform.position.y < position.y);
        }
        if (rotate_x)
        {
            rotated_x = (Player.localInstance.transform.position.x < position.x);
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
        Chunk.setBlock(position, GetMaterial(), stringFromData(data), true, false);
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

        Chunk.setBlock(position, Material.Air);
    }

    public virtual void Drop()
    {
        GetDrop().Drop(position);
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

            BreakIndicator.instance.UpdateState(spriteIndex, position);
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
            int textureIndex = new System.Random(Chunk.seedByPosition(position)).Next(0, alternative_textures.Length);

            return Resources.Load<Sprite>("Sprites/" + alternative_textures[textureIndex]);
        }
        else return Resources.Load<Sprite>("Sprites/" + texture);
    }

    public Material GetMaterial()
    {
        return (Material)System.Enum.Parse(typeof(Material), this.GetType().Name);
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