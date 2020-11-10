using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Particle : Entity
{
    private int bounces;
    public bool doGravity = true;
    public float maxAge = 5;
    public int maxBounces = 3;

    public Color color
    {
        get => GetRenderer().color;
        set => GetRenderer().color = value;
    }

    public Vector2 velocity
    {
        get => GetComponent<Rigidbody2D>().velocity;
        set => GetComponent<Rigidbody2D>().velocity = value;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        GetComponent<Rigidbody2D>().gravityScale = doGravity ? 1 : 0;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        StartCoroutine(ScheduleDeath());
    }

    IEnumerator ScheduleDeath()
    {
        yield return new WaitForSeconds(maxAge);
        
        Die();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        bounces++;
        if (bounces > maxBounces) Destroy(gameObject);
    }

    public override void Save()
    {
    }

    public override void Load()
    {
    }

    public override void UpdateEntityLightLevel()
    {
    }
    
    public static void Spawn_SmallSmoke(Vector2 position, Color color)
    {
        var rand = new Random();

        for (var x = 0; x < 4; x++)
        for (var y = 0; y < 4; y++)
            if (rand.NextDouble() < 0.2f)
            {
                var part = (Particle) Spawn("Particle");

                part.transform.position = position - new Vector2(0.5f, 0.5f) + new Vector2(0.25f * x, 0.25f * y);
                part.color = color;
                part.doGravity = false;
                part.velocity = new Vector2(0, 0.3f + (float) rand.NextDouble() * 0.5f);
                part.maxAge = 0.5f + (float) rand.NextDouble();
            }
    }

    public static void Spawn_Number(Vector2 position, int number, Color color)
    {
        var rand = new Random();
        var shape = new List<Vector2>();

        if (number == 1)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 2),
                new Vector2(0, 3), new Vector2(-1, 3)
            };
        if (number == 2)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 1),
                new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3)
            };
        if (number == 3)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3)
            };
        if (number == 4)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, 0), new Vector2(1, 2),
                new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3)
            };
        if (number == 5)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3), new Vector2(0, 3)
            };
        if (number == 6)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(-1, 1),
                new Vector2(0, 1), new Vector2(-1, 2), new Vector2(-1, 3)
            };
        if (number == 7)
            shape = new List<Vector2>
            {
                new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3),
                new Vector2(0, 3)
            };
        if (number > 7)
            shape = new List<Vector2>
            {
                new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3),
                new Vector2(0, 3), new Vector2(3, 1), new Vector2(4, 3), new Vector2(4, 4), new Vector2(4, 2),
                new Vector2(5, 3)
            };

        var totalVelocity = new Vector2(((float) rand.NextDouble() - 0.5f) * 0.5f, (float) rand.NextDouble() * 1f);
        foreach (var pos in shape)
        {
            var part = (Particle) Spawn("Particle");

            part.transform.position = position - new Vector2(0.5f, 0.5f) + pos * 0.25f;
            part.color = color;
            part.doGravity = true;
            part.velocity = totalVelocity;
            part.maxAge = 1f + (float) rand.NextDouble();
            part.maxBounces = 10;
        }
    }

    public override void EnterLiquid(Liquid liquid)
    {
        isInLiquid = true;
    }
}