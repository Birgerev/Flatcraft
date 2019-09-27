using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Entity : MonoBehaviour
{
    public Chunk currentChunk;

    public bool isOnGround;
    public float age;
    public bool isInLiquid = false;

    public string saveData;
    
    public virtual void Start()
    {

    }

    public virtual void Update()
    {
        age += Time.deltaTime;
        CheckChunk();
        checkSuffocation();
    }

    public virtual void CheckChunk()
    {
        //Get current chunk
        currentChunk = Chunk.GetChunk(Chunk.GetChunkPosFromWorldPosition((int)transform.position.x), false);

        //Freeze if no chunk is found
        if (WorldManager.instance.loadingProgress != 1)
            GetComponent<Rigidbody2D>().simulated = false;
        else if (currentChunk != null)
            GetComponent<Rigidbody2D>().simulated = (currentChunk.isLoaded);
        else GetComponent<Rigidbody2D>().simulated = false;
    }

    private void checkSuffocation()
    {
        if (Time.frameCount % (int)(0.75f / Time.deltaTime) == 1)
        {
            if (Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position))))
            {
                if (Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position))).playerCollide && !Chunk.getBlock(Vector2Int.RoundToInt(((Vector2)transform.position))).trigger)
                    TakeSuffocationDamage(1);
            }
        }
    }

    public virtual void TakeSuffocationDamage(float damage)
    {
        //Destroy entity immediately since it has no health
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
}
