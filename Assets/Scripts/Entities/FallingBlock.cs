using Mirror;
using UnityEngine;

public class FallingBlock : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] [SyncVar]
    public Material material = Material.Sand;

    //Entity State

    [Server]
    public override void Initialize()
    {
        //wait till the block that spawned the sand dissapears to begin simulating physics
        GetComponent<Rigidbody2D>().simulated = false; 
        
        if (material == Material.Air)
            Die();

        base.Initialize();
    }

    [Client]
    public override void ClientInitialize()
    {
        GetRenderer().sprite = new ItemStack(material).GetSprite();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        
        base.ClientInitialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //once the block that spawned the fallingSand is gone, begin simulating physics
        if (!GetComponent<Rigidbody2D>().simulated &&
            !Physics2D.OverlapBox(Location.GetPosition(), new Vector2(0.2f, 0.2f), 0)) 
            GetComponent<Rigidbody2D>().simulated = true;


        if ((isOnGround || isInLiquid) && GetComponent<Rigidbody2D>().velocity.y == 0 && age > 1f)
        {
            Block overlappingBlock = Location.GetBlock();
            if (overlappingBlock == null || overlappingBlock is Liquid)
                Location.SetMaterial(material).Tick();
            else
                new ItemStack(material, 1).Drop(Location);

            GetComponent<Rigidbody2D>().simulated = false;
            Die();
        }
    }

    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}