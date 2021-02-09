using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimedTNT : Entity
{
    //Entity Properties

    //Entity Data Tags
    [EntityDataTag(true)] public float fuse = 0;

    //Entity State
    public SpriteRenderer fuseBlinker;

    public void Awake()
    {
        GetComponent<Rigidbody2D>().simulated = false; //wait till the block that spawned the sand dissapears to begin simulating physics
    }

    public override void Start()
    {
        base.Start();
        
        Sound.Play(Location, "random/tnt_fuse", SoundType.Entities, 0.8f, 1.2f); //Play splash sound
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        base.Update();

        if (!GetComponent<Rigidbody2D>().simulated &&
            !Physics2D.OverlapBox(Location.GetPosition(), new Vector2(0.2f, 0.2f), 0)) //once the TNT block that spawned the primedTNT is gone, begin simulating physics
            GetComponent<Rigidbody2D>().simulated = true;

        if (age >= fuse)
        {
            Explosion.Create(Location, 5, 1);
            Die();
        }

        bool blinkerState = (Time.time % 0.5f * 2 <= 0.5f);
        fuseBlinker.color = (blinkerState) ? Color.white : Color.clear;
    }

    public override void Hit(float damage)
    {
        //Disabling Hit
    }
}
