using Mirror;
using UnityEngine;

public class DroppedItem : Entity
{
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
        if (item.material == Material.Air || item.amount <= 0)
            Die();

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        base.Initialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //Despawn
        if (age > 60 * 5)
            Die();

        if (isOnGround)
            GetComponent<Rigidbody2D>().velocity *= 0.95f;

        CheckTrigger();
    }

    [Server]
    private void CheckTrigger()
    {
        if (Time.time % 1f - Time.deltaTime > 0 || dead)
            return;

        Collider2D[] cols = Physics2D.OverlapBoxAll(triggerOffset + (Vector2)transform.position, triggerSize, 0);
        foreach (Collider2D col in cols)
        {
            if(col.gameObject == gameObject)
                continue;
            
            if (col.GetComponent<DroppedItem>() != null)
                if (col.GetComponent<DroppedItem>().item.material == item.material)
                {
                    if (age < col.GetComponent<DroppedItem>().age || col.GetComponent<DroppedItem>().dead)
                        return;

                    item.amount += col.GetComponent<DroppedItem>().item.amount;
                    col.GetComponent<DroppedItem>().Die();
                    return;
                }

            if (col.GetComponent<Player>() != null && age >= 0.5f)
                if (col.GetComponent<Player>().GetInventory().AddItem(item))
                {
                    Sound.Play(Location, "random/pickup_pop", SoundType.Entities, 0.7f, 1.3f);
                    Die();
                }
        }
    }
    
    [Client]
    public override void ClientUpdate()
    {
        GetRenderer().sprite = item.GetSprite();

        //Bobbing
        GetRenderer().transform.localPosition = new Vector3(0, (Mathf.Cos(cosIndex) * 0.1f) + 0.4f);
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