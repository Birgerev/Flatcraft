using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Entity : MonoBehaviour
{
    public static List<Entity> entities = new List<Entity>();

    public static int MaxLivingAmount = 4;
    private Vector2 _cachedposition;
    private Dimension _dimension;

    //Entity data tags
    [EntityDataTag(false)] public float age;
    [EntityDataTag(false)] public float fireTime;


    public Dictionary<string, string> data = new Dictionary<string, string>();
    public bool dead;
    public GameObject burningRender;

    [EntityDataTag(false)] public bool facingLeft;

    private bool hasInitializedLight;


    //Entity State
    public int id;
    public bool isInLiquid;
    public bool isOnGround;
    public bool isOnClimbable;
    public Vector2 lastFramePosition;
    public static int EntityCount => entities.Count;

    public static int LivingEntityCount
    {
        get
        {
            var i = 0;
            foreach (var entity in entities)
                if (entity is LivingEntity)
                    i++;
            return i;
        }
    }

    //Entity properties
    public virtual bool ChunkLoadingEntity { get; } = false;

    public Location Location
    {
        get => Location.LocationByPosition(_cachedposition, _dimension);
        set
        {
            transform.position = new Vector3(value.x, value.y);
            _dimension = value.dimension;
        }
    }

    public virtual void Start()
    {
        UpdateCachedPosition();
        gameObject.name = "Entity [" + GetType().Name + "]";
        entities.Add(this);

        Load();
        UpdateLight();
    }

    public virtual void Update()
    {
        age += Time.deltaTime;

        isInLiquid = (GetLiquidBlocksForEntity().Length > 0);
        
        if (isInLiquid)
            isOnGround = false;

        GetRenderer().flipX = facingLeft;

        UpdateCachedPosition();

        if (ChunkLoadingEntity)
            Chunk.CreateChunksAround(new ChunkPosition(Location), Chunk.RenderDistance);

        CheckLightUpdate();

        GetComponent<Rigidbody2D>().simulated = IsChunkLoaded();

        if (IsBurning())
        {
            ReduceFireTime();
            WaterRemoveFireTime();
        }
        DoFireRender();
        CheckWaterSplash();

        CheckFireDamage();
        CheckVoidDamage();
        CheckSuffocation();
        CheckLavaDamage();
    }

    public virtual void LateUpdate()
    {
        lastFramePosition = transform.position;
    }

    public void UpdateCachedPosition()
    {
        _cachedposition = transform.position;
    }

    public virtual bool IsBurning()
    {
        return (fireTime > 0);
    }

    public virtual bool IsChunkLoaded()
    {
        var result = new ChunkPosition(Location).IsChunkLoaded();

        //Freeze if no chunk is found
        if (WorldManager.instance.loadingProgress != 1)
            result = false;
        
        return result;
    }

    private void DoFireRender()
    {
        if(burningRender != null)
            burningRender.SetActive(IsBurning());
    }

    private void CheckLightUpdate()
    {
        if (Vector2Int.FloorToInt(lastFramePosition) != Vector2Int.FloorToInt(Location.GetPosition()))
            UpdateLight();
    }

    private void CheckWaterSplash()
    {
        if (isInLiquid && GetVelocity().y < -2)
        {
            bool isInWater = false;
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Water)
                {
                    isInWater = true;
                    break;
                }

            if (isInWater)
            {
                WaterSplash();
            }
        }
    }

    public virtual void WaterSplash()
    {
        var r = new Random();
        for (var i = 0; i < 8; i++) //Spawn landing partickes
        {
            Particle part = Particle.Spawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 0.5f);
            part.color = GetLiquidBlocksForEntity()[0].GetRandomColourFromTexture();
            part.doGravity = true;
            part.velocity = new Vector2(
                (1f + (float) r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                , 3f + (float) r.NextDouble());
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 10;
        }

        Sound.Play(Location, "entity/water_splash", SoundType.Entities, 0.75f, 1.25f); //Play splash sound
    }

    private void UpdateLight()
    {
        LightObject lightObj = GetRenderer().GetComponent<LightObject>();

        if (lightObj != null)
            LightManager.UpdateLightObject(lightObj);
    }

    private void WaterRemoveFireTime()
    {
        if(isInLiquid)
        {
            bool isInWater = false;
            foreach (Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Water)
                {
                    isInWater = true;
                    break;
                }

            if (isInWater)
            {
                fireTime = 0;
            }
        }
    }

    private void ReduceFireTime()
    {
        fireTime -= Time.deltaTime;
    }

    private void CheckSuffocation()
    {
        if (!IsChunkLoaded())
            return;

        if ((Time.time % 0.5f) - Time.deltaTime <= 0)
        {
            var block = Location.GetBlock();

            if (block != null)
                if (block.solid && !block.trigger && !(block is Liquid))
                    TakeSuffocationDamage(1);
        }
    }
    
    private void CheckFireDamage()
    {
        if(IsBurning())
            if ((Time.time % 1f) - Time.deltaTime <= 0)
                TakeFireDamage(1);
    }

    public virtual void TakeFireDamage(float damage)
    {
        Damage(damage);
    }

    private void CheckVoidDamage()
    {
        if ((Time.time % 0.5f) - Time.deltaTime <= 0)
            if (transform.position.y < 0)
                TakeVoidDamage(2);
    }

    public virtual void TakeVoidDamage(float damage)
    {
        Damage(damage);
    }

    private void CheckLavaDamage()
    {
        if (isInLiquid)
        {
            bool isInLava = false;
            foreach(Liquid liquid in GetLiquidBlocksForEntity())
                if (liquid is Lava)
                {
                    isInLava = true;
                    break;
                }

            if (isInLava)
            {
                fireTime = 14;
                
                if ((Time.time % 0.5f) - Time.deltaTime <= 0)
                {
                    TakeLavaDamage(4);
                }
            }
        }
    }

    public Liquid[] GetLiquidBlocksForEntity()
    {
        List<Liquid> liquids = new List<Liquid>();
        foreach (Collider2D col in Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0))
            if (col.GetComponent<Liquid>() != null)
                liquids.Add(col.GetComponent<Liquid>());

        return liquids.ToArray();
    }

    public virtual void TakeLavaDamage(float damage)
    {
        Damage(damage);
    }
    
    public virtual void TakeSuffocationDamage(float damage)
    {
        Damage(damage);
    }

    public virtual List<ItemStack> GetDrops()
    {
        var result = new List<ItemStack>();

        return result;
    }

    public virtual void DropAllDrops()
    {
        var i = 0;
        foreach (var item in GetDrops())
            if (item.material != Material.Air && item.amount > 0)
            {
                var random = new Random((transform.position + "" + i).GetHashCode());
                var maxVelocity = new Vector2(2, 2);
                var dropPosition = Location + new Location(0, 2);

                item.Drop(dropPosition,
                    new Vector2((float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x,
                        (float) random.NextDouble() * (maxVelocity.x - -maxVelocity.x) + -maxVelocity.x));

                i++;
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

    public virtual void SetVelocity(Vector2 velocity)
    {
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    public virtual Vector2 GetVelocity()
    {
        return GetComponent<Rigidbody2D>().velocity;
    }

    public virtual List<string> GetSaveStrings()
    {
        var result = new List<string>();
        result.Add("location=" + JsonUtility.ToJson(Location));
        var fields = GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (var field in fields)
        {
            var json = ((EntityDataTag) Attribute.GetCustomAttributes(field)[0]).json;

            if (json)
                result.Add(field.Name + "=" + JsonUtility.ToJson(field.GetValue(this)));
            else
                result.Add(field.Name + "=" + field.GetValue(this));
        }

        return result;
    }

    public virtual string SavePath()
    {
        return WorldManager.world.getPath() + "/region/" + Location.dimension + "/" +
               new ChunkPosition(Location).chunkX + "/entities/" + id + "." + GetType().Name;
    }

    public virtual void Save()
    {
        DeleteOldSavePath();

        var path = SavePath();

        if (!File.Exists(path))
            File.Create(path).Close();

        var lines = new List<string>();

        lines = GetSaveStrings();

        File.WriteAllLines(path, lines);
    }

    public virtual void DeleteOldSavePath()
    {
        File.Delete(SavePath());
    }

    public virtual SpriteRenderer GetRenderer()
    {
        return transform.Find("_renderer").GetComponent<SpriteRenderer>();
    }

    public virtual void Unload()
    {
        Save();

        entities.Remove(this);
        Destroy(gameObject, 0.2f);
    }

    public virtual void Load()
    {
        if (!HasBeenSaved())
            return;

        var lines = DataFromStrings(File.ReadAllLines(SavePath()));

        if (lines.Count <= 1)
            return;

        Location = JsonUtility.FromJson<Location>(lines["location"]);
        var fields = GetType().GetFields().Where(field => field.IsDefined(typeof(EntityDataTag), true));

        foreach (var field in fields)
        {
            var json = ((EntityDataTag) Attribute.GetCustomAttributes(field)[0]).json;
            var type = field.FieldType;

            if (json)
                field.SetValue(this, JsonUtility.FromJson(lines[field.Name], type));
            else if (type == typeof(string))
                field.SetValue(this, lines[field.Name]);
            else
                field.SetValue(this, Convert.ChangeType(lines[field.Name], type));
        }
    }

    public bool HasBeenSaved()
    {
        return File.Exists(SavePath());
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y && Mathf.Abs(col.transform.position.x - transform.position.x) < 0.9f && !isInLiquid)
            isOnGround = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y)
            isOnGround = false;
    }

    public void PlayCriticalDamageEffect()
    {
        var r = new Random();
        for (var i = 0; i < r.Next(2, 8); i++) //SpawnParticles
        {
            Particle part = Particle.Spawn();

            part.transform.position = Location.GetPosition() + new Vector2(0, 1f);
            part.color = new Color(0.854f, 0.788f, 0.694f);
            part.doGravity = true;
            part.velocity = new Vector2(
                (2f + (float)r.NextDouble()) * (r.Next(0, 2) == 0 ? -1 : 1)
                , 4f + (float)r.NextDouble());
            part.maxAge = 1f + (float)r.NextDouble();
            part.maxBounces = 10;
        }
    }

    public static int CreateId()
    {
        return UnityEngine.Random.Range(1, 99999);
    }

    public virtual void TeleportNetherPortal()
    {
        StartCoroutine(teleportNetherPortal());
    }

    IEnumerator teleportNetherPortal()
    {
        //Translate dimension coordinates
        Dimension currentDimension = Location.dimension;
        Location newLocation = new Location(0, 300);

        if (currentDimension == Dimension.Overworld)
        {
            newLocation.x = Mathf.FloorToInt((float)Location.x / 8f);
            newLocation.dimension = Dimension.Nether;
        }
        else if (currentDimension == Dimension.Nether)
        {
            newLocation.x = Location.x * 8;
            newLocation.dimension = Dimension.Overworld;
        }

        //Load chunk in the other dimension
        Location = newLocation;                 //teleport player so chunk doesn't unload
        ChunkPosition cPos = new ChunkPosition(newLocation);
        Chunk chunk = cPos.CreateChunk();

        //Wait for chunk to load
        while (!chunk.isLoaded)
            yield return new WaitForSeconds(0.5f);

        Location portal;
        if (chunk.netherPortal != null)
            portal = chunk.netherPortal.location;
        else
            portal = chunk.GeneratePortal(newLocation.x);

        //Teleport player to new y value
        if(this.GetType().IsSubclassOf(typeof(LivingEntity)))
            ((LivingEntity)this).highestYlevelsinceground = 0;
        Location = portal;
    }

    public static Entity Spawn(string type)
    {
        if (Resources.Load("Entities/" + type) == null)
        {
            Debug.LogError("No Entity with the type '" + type + "' was found");
            return null;
        }

        var obj = Instantiate((GameObject) Resources.Load("Entities/" + type));
        var result = obj.GetComponent<Entity>();

        result.id = CreateId();

        return result;
    }

    public static Dictionary<string, string> DataFromStrings(string[] dataStrings)
    {
        var resultData = new Dictionary<string, string>();

        foreach (var line in dataStrings)
            if (line.Contains("="))
                resultData.Add(line.Split('=')[0], line.Split('=')[1]);

        return resultData;
    }
}

[AttributeUsage(AttributeTargets.All)]
public class EntityDataTag : Attribute
{
    public readonly bool json;

    public EntityDataTag(bool json)
    {
        this.json = json;
    }
}