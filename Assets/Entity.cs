using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[System.Serializable]
public class Entity : MonoBehaviour
{
    //Entity properties
    public virtual bool chunk_loading { get; } = false;

    //Entity data tags
    [EntityDataTag(false)]
    public float age = 0;

    //Entity State
    public Chunk currentChunk;
    public bool isOnGround;
    public bool isInLiquid = false;


    public Dictionary<string, string> data = new Dictionary<string, string>();

    public virtual void Start()
    {
        gameObject.name = "Entity ["+this.GetType().Name+"]";
    }

    public virtual void Update()
    {
        age += Time.deltaTime;

        

        CheckChunk();
        checkVoidDamage();
        checkSuffocation();
    }

    public virtual bool CheckChunk()
    {
        bool result = false;

        //Get current chunk
        currentChunk = Chunk.GetChunk(Chunk.GetChunkPosFromWorldPosition((int)transform.position.x), chunk_loading);

        //Freeze if no chunk is found
        if (WorldManager.instance.loadingProgress != 1)
            result = false;
        else if (currentChunk != null)
            result = (currentChunk.isLoaded);
        else result = false;

        GetComponent<Rigidbody2D>().simulated = result;
        return result;
    }

    private void checkSuffocation()
    {
        if (!CheckChunk())
            return;

        if (Time.frameCount % (int)(0.75f / Time.deltaTime) == 1)
        {
            if (Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position)))!= null)
            {
                if (Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position))).playerCollide && !Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position))).trigger)
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

    public virtual void Damage(float damage)
    {
        Destroy(gameObject);
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

        result.Add("position=" + JsonUtility.ToJson(transform.position));

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
        return WorldManager.world.getPath() + "\\TODO\\";
    }

    public virtual void Save()
    {

    }


    public virtual SpriteRenderer getRenderer()
    {
        return GetComponent<SpriteRenderer>();
    }

    public virtual void Load()
    {
        if (!File.Exists(SavePath()))
            return;

        Dictionary<string, string> lines = dataFromStrings(File.ReadAllLines(SavePath()));

        if (lines.Count <= 1)
            return;

        transform.position = JsonUtility.FromJson<Vector3>(lines["position"]);


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
        if(col.transform.position.y+1f < transform.position.y && Mathf.Abs(col.transform.position.x - transform.position.x) < 0.9f)
            isOnGround = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.transform.position.y + 1f < transform.position.y)
            isOnGround = false;
    }

    public static Entity Spawn(string id)
    {
        if(Resources.Load("Entities/" + id) == null)
        {
            Debug.LogError("No Entity with the id '"+id+"' was found");
            return null;
        }

        GameObject obj = Instantiate((GameObject)Resources.Load("Entities/"+id));

        return obj.GetComponent<Entity>();
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