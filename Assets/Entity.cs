using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Random = UnityEngine.Random;

[System.Serializable]
public class Entity : MonoBehaviour
{
    public static List<Entity> entities = new List<Entity>();
    public static int entityCount { get { return entities.Count; } }
    public static int livingEntityCount { get {
            int i = 0;
            foreach (Entity entity in entities)
            {
                if (entity is LivingEntity)
                    i++;
            }
            return i; } }
     
    public static int MaxLivingAmount = 6;

    //Entity properties
    public virtual bool ChunkLoadingEntity { get; } = false;

    //Entity data tags
    [EntityDataTag(false)]
    public float age = 0;


    //Entity State
    public int id = 0;
    public Chunk currentChunk;
    public bool isOnGround;
    public bool isInLiquid = false;
    public bool isOnLadder = false;
    public bool flipRenderX = false;
    public bool dead = false;
    public Vector2 lastFramePosition;
    public Location location
    {
        get
        {
            return Location.locationByPosition(_cachedposition, _dimension);
        }
        set
        {
            transform.position = new Vector3(value.x, value.y);
            _dimension = value.dimension;
        }
    }
    private Vector2 _cachedposition;
    private Dimension _dimension;


    public Dictionary<string, string> data = new Dictionary<string, string>();

    public virtual void Start()
    {
        gameObject.name = "Entity ["+this.GetType().Name+"]";
        entities.Add(this);

        Load();
    }

    public virtual void Update()
    {
        age += Time.deltaTime;

        if (isInLiquid)
            isOnGround = false;

        getRenderer().flipX = flipRenderX;

        UpdateCachedPosition();
        currentChunk = Chunk.GetChunk(new ChunkPosition(location), ChunkLoadingEntity);

        GetComponent<Rigidbody2D>().simulated = isChunkLoaded();
        checkVoidDamage();
        checkSuffocation();
    }

    public void LateUpdate()
    {
        lastFramePosition = transform.position;
    }

    public void UpdateCachedPosition()
    {
        _cachedposition = transform.position;
    }

    public virtual bool isChunkLoaded()
    {
        bool result = false;

        //Freeze if no chunk is found
        if (WorldManager.instance.loadingProgress != 1)
            result = false;
        else if (currentChunk != null)
            result = (currentChunk.isLoaded);
        else result = false;
        
        return result;
    }

    private void checkSuffocation()
    {
        if (!isChunkLoaded())
            return;

        if (Time.frameCount % (int)(0.75f / Time.deltaTime) == 1)
        {
            Block block = Chunk.getBlock(location);

            if (block != null)
            {
                if (block.playerCollide && !block.trigger && !(block is Liquid))
                    TakeSuffocationDamage(1);
            }
        }
    }

    private void checkVoidDamage()
    {
        if (Time.frameCount % (int)(0.75f / Time.deltaTime) == 1)
        {
            if (transform.position.y < 0)
            {
                TakeVoidDamage(1);
            }
        }
    }
    
    public virtual void TakeVoidDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void TakeSuffocationDamage(float damage)
    {
        Damage(damage);
    }

    public virtual List<ItemStack> GetDrops()
    {
        List<ItemStack> result = new List<ItemStack>();

        return result;
    }

    public virtual void DropAllDrops()
    {
        int i = 0;
        foreach (ItemStack item in GetDrops())
        {
            if (item.material != Material.Air && item.amount > 0)
            {
                System.Random random = new System.Random((transform.position + "" + i).GetHashCode());
                Vector2 maxVelocity = new Vector2(2, 2);
                Location dropPosition = location + new Location(0, 2);

                item.Drop(dropPosition,
                    new Vector2((float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
                    (float)random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x));

                i++;
            }
        }
    }

    public virtual void Die()
    {
        DropAllDrops();

        DeleteOldSavePath();

        dead = true;
        entities.Remove(this);

        Destroy(gameObject, 0.2f);
    }

    public virtual void Damage(float damage)
    {
        Die();
    }

    public virtual void Hit(float damage)
    {
        TakeHitDamage(damage);
    }

    public virtual void TakeHitDamage(float damage)
    {
        Damage(damage);
    }

    public virtual void setVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public virtual Vector2 getVelocity()
    {
        return GetComponent<Rigidbody2D>().velocity;
    }

    public virtual List<string> GetSaveStrings()
    {
        List<string> result = new List<string>();

        result.Add("location=" + JsonUtility.ToJson(location));

        IEnumerable<System.Reflection.FieldInfo> fields = this.GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (System.Reflection.FieldInfo field in fields)
        {
            bool json = ((EntityDataTag)System.Attribute.GetCustomAttributes(field)[0]).json;

            if (json)
                result.Add(field.Name + "=" + JsonUtility.ToJson(field.GetValue(this)));
            else
                result.Add(field.Name + "=" + field.GetValue(this).ToString());
        }

        return result;
    }

    public virtual string SavePath()
    {
        return WorldManager.world.getPath() + "\\region\\" + location.dimension.ToString() + "\\"+(new ChunkPosition(location)).chunkX+"\\entities\\"+id+"."+GetType().Name;
    }

    public virtual void Save()
    {
        DeleteOldSavePath();

        string path = SavePath();

        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }

        List<string> lines = new List<string>();

        lines = GetSaveStrings();

        File.WriteAllLines(path, lines);
    }

    public virtual void DeleteOldSavePath()
    {
        string[] chunks = Directory.GetDirectories(WorldManager.world.getPath() + "\\region\\"+location.dimension.ToString());
            
        foreach (string chunk in chunks)
        {
            string[] entities = Directory.GetFiles(chunk + "\\entities\\");

            foreach (string entity in entities)
            {
                if (int.Parse(entity.Split('\\')[entity.Split('\\').Length - 1].Split('.')[0]) == id)
                {
                    File.Delete(entity);
                }
            }
        }
    }

    public virtual SpriteRenderer getRenderer()
    {
        return transform.Find("_renderer").GetComponent<SpriteRenderer>();
    }

    public virtual void Load()
    {
        if (!File.Exists(SavePath()))
            return;

        Dictionary<string, string> lines = dataFromStrings(File.ReadAllLines(SavePath()));

        if (lines.Count <= 1)
            return;

        location = JsonUtility.FromJson<Location>(lines["location"]);


        IEnumerable<System.Reflection.FieldInfo> fields = this.GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (System.Reflection.FieldInfo field in fields)
        {
            bool json = ((EntityDataTag)System.Attribute.GetCustomAttributes(field)[0]).json;
            System.Type type = field.FieldType;

            if(json)
                field.SetValue(this, JsonUtility.FromJson(lines[field.Name], type));
            else if(type == typeof(string))
                field.SetValue(this, lines[field.Name]);
            else
                field.SetValue(this, System.Convert.ChangeType(lines[field.Name], type));
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if(col.transform.position.y+1f < transform.position.y && Mathf.Abs(col.transform.position.x - transform.position.x) < 0.9f && !isInLiquid)
            isOnGround = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y)
            isOnGround = false;
    }

    public virtual void EnterLiquid(Liquid liquid)
    {
        isInLiquid = true;

        if (getVelocity().y < -2)
        {
            System.Random r = new System.Random();
            for (int i = 0; i < 8; i++) //Spawn landing partickes
            {
                Particle part = (Particle) Entity.Spawn("Particle");

                part.transform.position = liquid.location.getPosition() + new Vector2(0, 0.5f);
                part.color = liquid.GetRandomColourFromTexture();
                part.doGravity = true;
                part.velocity = new Vector2(
                    (1f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                    , 3f + (float) r.NextDouble());
                part.maxAge = 1f + (float) r.NextDouble();
                part.maxBounces = 10;
            }

            Debug.Log("Playing Sound");
            Sound.Play(location, "entity/water_splash", SoundType.Entities, 0.75f, 1.25f); //Play splash sound
        }
    }

    public virtual void ExitLiquid(Liquid liquid)
    {
        isInLiquid = false;
    }

    public static int CreateId()
    {
        return Random.Range(1, 99999);
    }

    public static Entity Spawn(string type)
    {
        if(Resources.Load("Entities/" + type) == null)
        {
            Debug.LogError("No Entity with the type '" + type + "' was found");
            return null;
        }

        GameObject obj = Instantiate((GameObject)Resources.Load("Entities/"+ type));
        Entity result = obj.GetComponent<Entity>();

        result.id = CreateId();

        return result;
    }

    public static Dictionary<string, string> dataFromStrings(string[] dataStrings)
    {
        Dictionary<string, string> resultData = new Dictionary<string, string>();

        foreach (string line in dataStrings)
        {
            if (line.Contains("="))
                resultData.Add(line.Split('=')[0], line.Split('=')[1]);
        }

        return resultData;
    }
}

[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
public class EntityDataTag : System.Attribute
{
    public bool json;

    public EntityDataTag(bool Json)
    {
        json = Json;
    }
}