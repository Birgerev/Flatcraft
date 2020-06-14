using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public class Block : MonoBehaviour
{
    public static Dictionary<Block, int> lightSources = new Dictionary<Block, int>();
    public static HashSet<Block> sunlightSources = new HashSet<Block>();

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
    
    public virtual Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;

    public virtual int glowLevel { get; } = 0;

    public Dictionary<string, string> data = new Dictionary<string, string>();

    public float blockHealth = 0;
    private bool firstTick = true;

    public Location location;
    public int age = 0;

    private float time_of_last_hit = 0;
    private float time_of_last_autosave = 0;
    public virtual void Initialize()
    {
        gameObject.name = "block [" + transform.position.x + "," + transform.position.y + "]";
        blockHealth = breakTime;

        texture = (string)this.GetType().
            GetField("default_texture", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);


        UpdateColliders();

        RenderNoLight();

        //Cache position for use in multithreading
        location = Location.locationByPosition(transform.position, location.dimension);

        if (autoTick || autosave)
            StartCoroutine(autoTickLoop());

        Render();
    }

    public virtual void FirstTick()
    {
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
        if(age == 0 && spread)    //Block place sound
        {
            Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Blocks, 0.5f, 1.5f);
        }

        if (requiresGround)
        {
            if (Chunk.getBlock(location - new Location(0, 1)) == null)
            {
                Break();
            }
        }

        UpdateColliders();

        RenderRotate();


        if (Time.time - time_of_last_autosave > Chunk.AutosaveDuration && autosave && blockHealth == breakTime && age > 1)
        {
            Autosave();
            return;
        }

        age++;
        if (spread)
            SpreadTick(location);
    }

    public float getRandomChance()
    {
        return (float)new System.Random(Chunk.seedByLocation(location) + age).NextDouble();
    }

    IEnumerator autoTickLoop()
    {
        //Wait a random duration, to smooth out ticks across time
        yield return new WaitForSeconds((float)new System.Random(Chunk.seedByLocation(location)).NextDouble() * (1f / Chunk.TickRate));

        while (true)
        {
            yield return new WaitForSeconds(1 / Chunk.TickRate);
            if(GetChunk().isTickedChunk)
                Tick(false);
        }
    }
    
    public static void SpreadTick(Location loc)
    {
        List<Block> blocks = new List<Block>();

        blocks.Add(Chunk.getBlock(loc + new Location(0, 1)));
        blocks.Add(Chunk.getBlock(loc + new Location(0, -1)));
        blocks.Add(Chunk.getBlock(loc + new Location(-1, 0)));
        blocks.Add(Chunk.getBlock(loc + new Location(1, 0)));

        foreach (Block block in blocks) {
            if (block != null)
            {
                block.Tick(false);
            }
        }
    }
    
    public static void UpdateSunlightSourceAt(int x, Dimension dimension)
    {
        Block topBlock = Chunk.getTopmostBlock(x, dimension);
        bool isDay = (WorldManager.world.time % WorldManager.dayLength) < (WorldManager.dayLength / 2);
        if (topBlock == null)
            return;

        //remove all sunlight sources in the same column
        foreach(Block source in sunlightSources.ToList())
        {
            if (source.location.x == x && source.location.dimension == dimension)
            {
                sunlightSources.Remove(source);
                lightSources.Remove(source);
                UpdateLightAround(source.location);
            }
        }

        //Add the new position
        sunlightSources.Add(topBlock);
        lightSources.Add(topBlock, isDay ? 15 : 5);
        UpdateLightAround(topBlock.location);
    }

    public bool IsSunlightSource()
    {
        return sunlightSources.Contains(this);
    }

    public bool CheckBlockLightSource()
    {
        if (glowLevel > 0)
        {
            lightSources[this] = glowLevel;
        }

        return GetLightSourceLevel(this) > 0;
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

    public static void UpdateLightAround(Location loc)
    {
        Block source = Chunk.getBlock(loc);
        if(source != null)
        {
            source.CheckBlockLightSource();
        }
        
        Chunk chunk = Chunk.GetChunk(new ChunkPosition(loc), false);
        if(chunk != null)
            chunk.lightSourceToUpdate.Add(loc);
    }

    public static int GetLightSourceLevel(Block block)
    {
        if (block == null)
            return 0;
        
        bool isDay = (WorldManager.world.time % WorldManager.dayLength) < (WorldManager.dayLength / 2);
        return block.IsSunlightSource() ? (isDay ? 15 : 5) : block.glowLevel;
    }

    public static int GetLightLevel(Location loc)
    {
        //Messy layout due to multithreading
        if (lightSources.Count <= 0 && sunlightSources.Count <= 0)
            return 0;
        
        List<Block> sources;
        lock (lightSources)
        {
            //clone actual list to avoid threading errors
            sources = new Dictionary<Block, int>(lightSources).Keys.ToList();
        }

        Location brightestSourceLoc = new Location(0, 0);
        int brightestValue = 0;
        
        foreach (Block source in sources)
        {
            Location sourceLoc = source.location;
            if (sourceLoc.dimension == loc.dimension)
            {
                int sourceBrightness = GetLightSourceLevel(source);
                int value = sourceBrightness - (int)(math.distance(sourceLoc.getPosition(), loc.getPosition()));

                if (value > brightestValue)
                {
                    brightestValue = value;
                    brightestSourceLoc = sourceLoc;
                }
            }
        }
        
        return brightestValue;
    }

    public virtual void UpdateColliders()
    {
        GetComponent<Collider2D>().enabled = (playerCollide || trigger);
        GetComponent<Collider2D>().isTrigger = (trigger);
    }

    public Color GetRandomColourFromTexture()
    {
        Texture2D texture = getTexture().texture;
        Color[] pixels = texture.GetPixels();
        System.Random random = new System.Random();

        return pixels[random.Next(pixels.Length)];
    }

    public void Rotate()
    {
        bool rotated_x = false;
        bool rotated_y = false;

        if (rotate_y)
        {
            rotated_y = (Player.localInstance.transform.position.y < location.y);
        }
        if (rotate_x)
        {
            rotated_x = (Player.localInstance.transform.position.x < location.x);
        }

        data["rotated_x"] = rotated_x ? "true" : "false";
        data["rotated_y"] = rotated_y ? "true" : "false";
        
        //Save new rotation
        Autosave();
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
        Chunk chunk = Chunk.GetChunk(new ChunkPosition(location), false);
        time_of_last_autosave = Time.time;

        chunk.SaveBlock(this);
    }

    public virtual void Hit(float time)
    {
        Hit(time, Tool_Type.None, Tool_Level.None);
    }

    public virtual void Hit(float time, Tool_Type tool_type, Tool_Level tool_level)
    {
        time_of_last_hit = Time.time;

        bool properToolStats = false;

        if(tool_level != Tool_Level.None && tool_type == propperToolType && tool_level >= propperToolLevel)
        {
            time *= 2 + ((float)tool_level * 2f);
        }
        if (propperToolLevel == Tool_Level.None ||
            (tool_type == propperToolType && tool_level >= propperToolLevel))
        {
            properToolStats = true;
        }

        blockHealth -= time;

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/hit", SoundType.Blocks, 0.8f, 1.2f);

        RenderBlockDamage();

        if (blockHealth <= 0)
        {
            if (properToolStats)
                Break();
            else
                Break(false);

            Player.localInstance.DoToolDurability();

            return;
        }

        RenderBlockDamage();
        StartCoroutine(repairOnceViable());
    }

    IEnumerator repairOnceViable()
    {
        while (Time.time - time_of_last_hit < 1)
        {
            yield return new WaitForSeconds(0.2f);
        }

        blockHealth = breakTime;
    }


    public virtual void Break()
    {
        Break(true);
    }

    public Chunk GetChunk()
    {
        return transform.parent.GetComponent<Chunk>();
    }

    public virtual void Break(bool drop)
    {
        if (drop)
            Drop();

        Sound.Play(location, "block/" + blockSoundType.ToString().ToLower() + "/break", SoundType.Blocks, 0.5f, 1.5f);

        Chunk.setBlock(location, Material.Air);
    }

    public virtual void Drop()
    {
        GetDrop().Drop(location);
    }

    public virtual ItemStack GetDrop()
    {
        return new ItemStack(GetMaterial(), 1);
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

        if (blockHealth != breakTime)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Block_Break");
            int spriteIndex = (int)((blockHealth/breakTime) / (1f / ((float)sprites.Length)));

            BreakIndicator.instance.UpdateState(spriteIndex, location);
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
            int textureIndex = new System.Random(Chunk.seedByLocation(location)).Next(0, alternative_textures.Length);

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
        foreach (KeyValuePair<string, string> entry in data)
        {
            if (!first)
                result += "|";
            result += entry.Key + "=" + entry.Value; 
            first = false;
        }

        return result;
    }
}

public enum Block_SoundType
{
    Stone,
    Wood,
    Sand,
    Dirt,
    Grass,
    Wool,
    Gravel
}