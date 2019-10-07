using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : Entity
{
    public Color color { get { return GetComponent<SpriteRenderer>().color; } set { GetComponent<SpriteRenderer>().color = value; } }
    public Vector2 velocity { get { return GetComponent<Rigidbody2D>().velocity; } set { GetComponent<Rigidbody2D>().velocity = value; } }
    public int maxBounces = 3;

    private int bounces = 0;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
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
}