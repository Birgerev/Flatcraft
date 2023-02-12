using System.Linq;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class DroppedItem : Entity
{
    private static readonly Vector2 SpawnVelocity = new Vector2(1.5f, 3.5f);
    private const float BobAmplitude = 0.1f;
    private const float BobOffset = 0.35f;
    //Entity State
    private float cosIndex;
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] [SyncVar] public ItemStack item = new ItemStack();
    public Vector2 triggerOffset;
    public Vector2 triggerSize;

    [Server]
    public override void Initialize()
    {
        if (item.material == Material.Air || item.Amount <= 0)
            Remove();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        base.Initialize();
    }

    [Server]
    public override void Spawn()
    {
        base.Spawn();
    
        //If entity has no velocity when spawned,
        if (GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f)
        {
            //Apply velocity left or right randomly
            Vector2 velocity = SpawnVelocity;
            if (new Random().NextDouble() < 0.5f)
                velocity.x *= -1;
            GetComponent<Rigidbody2D>().velocity += velocity;
        }
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //Despawn
        if (age > 60 * 5)
            Remove();

        if (isOnGround)
            GetComponent<Rigidbody2D>().velocity *= 0.93f;
        //TODO hitbox

        CheckTrigger();
    }

    [Server]
    private void CheckTrigger()
    {
        if (Time.time % .2f - Time.deltaTime > 0 || dead)
            return;

        Collider2D[] cols = Physics2D.OverlapBoxAll(triggerOffset + (Vector2)transform.position, triggerSize, 0);
        Random r = new Random(); // Shuffle collider array order, as to not give priorities to certain players
        cols = cols.OrderBy(x => r.Next()).ToArray(); 
        
        foreach (Collider2D col in cols)
        {
            if(col.gameObject == gameObject)
                continue;
            
            if (col.GetComponent<DroppedItem>() != null)
                if (col.GetComponent<DroppedItem>().item.material == item.material)
                {
                    if (age < col.GetComponent<DroppedItem>().age || col.GetComponent<DroppedItem>().dead)
                        return;

                    item.Amount += col.GetComponent<DroppedItem>().item.Amount;
                    col.GetComponent<DroppedItem>().Remove();
                    return;
                }

            if (col.GetComponent<Player>() != null && age >= 0.5f)
                if (col.GetComponent<Player>().GetInventory().AddItem(item))
                {
                    Sound.Play(Location, "random/pickup_pop", SoundType.Entities, 0.7f, 1.3f);
                    Remove();
                    return;
                }
        }
    }
    
    [Client]
    public override void ClientUpdate()
    {
        //Item color
        GetRenderer().color = item.GetTextureColors()[0];

        //Bobbing
        GetRenderer().transform.localPosition = new Vector3(0, (Mathf.Cos(cosIndex) * BobAmplitude) + BobOffset);
        cosIndex += 2f * Time.deltaTime;

        base.ClientUpdate();
    }

    public override void TakeSuffocationDamage(float damage)
    {
        //Disable suffocation damage and float upwards instead
        Teleport(Location + new Location(0, 1));
    }


    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}