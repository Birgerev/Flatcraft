using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : Entity
{
    public Color color { get { return getRenderer().color; } set { getRenderer().color = value; } }
    public Vector2 velocity { get { return GetComponent<Rigidbody2D>().velocity; } set { GetComponent<Rigidbody2D>().velocity = value; } }
    public int maxBounces = 3;
    public float maxAge = 5;
    public bool doGravity = true;

    private int bounces = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        GetComponent<Rigidbody2D>().gravityScale = doGravity ? 1 : 0;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        Destroy(gameObject, maxAge);
    }

    public override void Update()
    {

        base.Update();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        bounces++;
        if(bounces > maxBounces)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    public override void Save()
    {
    }

    // Update is called once per frame
    public override void Load()
    {
    }

    public static void Spawn_SmallSmoke(Vector2 position, Color color)
    {
        System.Random rand = new System.Random();

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (rand.NextDouble() < 0.2f)
                {
                    Particle part = (Particle)Entity.Spawn("Particle");

                    part.transform.position = (position - new Vector2(0.5f, 0.5f)) + new Vector2(0.25f * x, 0.25f * y);
                    part.color = color;
                    part.doGravity = false;
                    part.velocity = new Vector2(0, 0.3f + (float)rand.NextDouble() * 0.5f);
                    part.maxAge = 0.5f + (float)rand.NextDouble();
                }
            }
        }
    }

    public static void Spawn_Number(Vector2 position, int number, Color color)
    {
        System.Random rand = new System.Random();
        List<Vector2> shape = new List<Vector2>();

        if(number == 1)
            shape = new List<Vector2>() { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 2), new Vector2(0, 3), new Vector2(-1, 3) };
        if (number == 2)
            shape = new List<Vector2>() { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3) };
        if (number == 3)
            shape = new List<Vector2>() { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3) };
        if (number == 4)
            shape = new List<Vector2>() { new Vector2(1, 0), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, 0), new Vector2(1, 2), new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3) };
        if (number == 5)
            shape = new List<Vector2>() { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3), new Vector2(0, 3) };
        if (number == 6)
            shape = new List<Vector2>() { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(0, 1), new Vector2(-1, 2), new Vector2(-1, 3) };
        if (number == 7)
            shape = new List<Vector2>() { new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3), new Vector2(0, 3) };
        if (number > 7)
            shape = new List<Vector2>() { new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3), new Vector2(0, 3),  new Vector2(3, 1), new Vector2(4, 3), new Vector2(4, 4), new Vector2(4, 2), new Vector2(5, 3) };

        Vector2 totalVelocity = new Vector2(((float)rand.NextDouble() - 0.5f) * 0.5f, (float)rand.NextDouble() * 1f);
        foreach (Vector2 pos in shape)
        {
            Particle part = (Particle)Entity.Spawn("Particle");

            part.transform.position = (position - new Vector2(0.5f, 0.5f)) + (pos*0.25f);
            part.color = color;
            part.doGravity = true;
            part.velocity = totalVelocity;
            part.maxAge = 1f + (float)rand.NextDouble();
            part.maxBounces = 10;
        }
    }
}