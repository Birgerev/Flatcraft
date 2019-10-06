using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Entity : MonoBehaviour
{
    public Chunk currentChunk;

    public virtual bool chunk_loading { get; } = false;

    public bool isOnGround;
    public float age;
    public bool isInLiquid = false;

    public string saveData;
    
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

    public virtual void Save()
    {

    }

    public virtual SpriteRenderer getRenderer()
    {
        return GetComponent<SpriteRenderer>();
    }

    public virtual void Load()
    {

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
}
