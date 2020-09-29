using UnityEngine;

public class FallingSand : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] public Material material = Material.Sand;

    //Entity State

    public void Awake()
    {
        GetComponent<Rigidbody2D>().simulated =
            false; //wait till the block that spawned the sand dissapears to begin simulating physics
    }

    public override void Start()
    {
        base.Start();

        if (material == Material.Air)
            Die();

        GetRenderer().sprite = new ItemStack(material).GetSprite();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        if (!GetComponent<Rigidbody2D>().simulated &&
            !Physics2D.OverlapBox(Location.GetPosition(), new Vector2(0.2f, 0.2f), 0)
        ) //once the block that spawned the fallingSand is gone, begin simulating physics
            GetComponent<Rigidbody2D>().simulated = true;


        if ((isOnGround || isInLiquid && GetComponent<Rigidbody2D>().velocity.y == 0) && age > 1f && !dead)
        {
            if (Location.GetMaterial() == Material.Air)
                Location.SetMaterial(material);
            else
                new ItemStack(material, 1).Drop(Location);

            Die();
        }
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}