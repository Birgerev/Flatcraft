using Mirror;
using UnityEngine;

public class PrimedTNT : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] public float fuse;

    //Entity State
    public SpriteRenderer fuseBlinker;

    [Server]
    public override void Initialize()
    {
        GetComponent<Rigidbody2D>().simulated =
            false; //wait till the block that spawned the sand dissapears to begin simulating physics
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        Sound.Play(Location, "random/tnt_fuse", SoundType.Entities, 0.8f, 1.2f); //Play splash sound

        base.Initialize();
    }

    [Server]
    public override void Tick()
    {
        base.Tick();

        //once the TNT block that spawned the primedTNT is gone, begin simulating physics
        if (!GetComponent<Rigidbody2D>().simulated &&
            !Physics2D.OverlapBox(Location.GetPosition(), new Vector2(0.2f, 0.2f), 0))
            GetComponent<Rigidbody2D>().simulated = true;

        if (age >= fuse)
        {
            Explosion.Create(Location, 4, 1);
            Remove();
        }
    }

    [Client]
    public override void ClientUpdate()
    {
        base.ClientUpdate();

        bool blinkerState = Time.time % 0.5f * 2 <= 0.5f;
        fuseBlinker.color = blinkerState ? Color.white : Color.clear;
    }

    [Server]
    public override void Hit(float damage, Entity source)
    {
        //Disabling Hit
    }
}