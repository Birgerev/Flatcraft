using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class Particle : MonoBehaviour
{
    public bool doGravity = true;
    public float maxAge = 5;
    public int maxBounces = 3;
    public string spriteSheet = "particle_dot";
    public int animationLength = -1;
    public float animationDuration = -1;
    private int bounces;

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
    public void Start()
    {
        GetComponent<Rigidbody2D>().gravityScale = doGravity ? 1 : 0;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        Destroy(gameObject, maxAge);

        if (animationLength != -1)
            StartCoroutine(ParticleAnimation());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        bounces++;
        if (bounces > maxBounces)
            Destroy(gameObject);
    }

    private IEnumerator ParticleAnimation()
    {
        Sprite[] frames = Resources.LoadAll<Sprite>("Sprites/" + spriteSheet);
        int frameIndex = 0;
        while (frameIndex < animationLength)
        {
            GetRenderer().sprite = frames[frameIndex];
            frameIndex++;

            yield return new WaitForSeconds(animationDuration / animationLength);
        }

        Destroy(gameObject);
    }

    public virtual SpriteRenderer GetRenderer()
    {
        return transform.Find("_renderer").GetComponent<SpriteRenderer>();
    }

    public static Particle ClientSpawn()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Particle");
        GameObject partObj = Instantiate(prefab);
        Particle part = partObj.GetComponent<Particle>();

        return part;
    }

    public static void ClientSpawnSmallSmoke(Vector2 position, Color color)
    {
        //TODO replace with ClientSpawnVolume
        Random rand = new Random();

        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                if (rand.NextDouble() < 0.2f)
                {
                    Particle part = ClientSpawn();

                    part.transform.position = position - new Vector2(0.5f, 0.5f) + new Vector2(0.25f * x, 0.25f * y);
                    part.color = color;
                    part.doGravity = false;
                    part.velocity = new Vector2(0, 0.3f + (float) rand.NextDouble() * 0.5f);
                    part.maxAge = 0.5f + (float) rand.NextDouble();
                }
    }
    
    public static List<Particle> ClientSpawnVolume(Vector2 position, Vector2 boundsSize, Vector2 maxVelocity, float2 ageRange, int2 amountRange, Color color)
    {
        List<Particle> particles = new List<Particle>();
        Random rand = new Random();
        int particleAmount = rand.Next(amountRange.x, amountRange.y + 1);
        
        for (int i = 0; i < particleAmount; i++)
        {
            Particle part = ClientSpawn();
            particles.Add(part);

            part.transform.position = position + new Vector2(
                (float)rand.NextDouble() * boundsSize.x, 
                (float)rand.NextDouble() * boundsSize.y);
            part.velocity = new Vector2(
                (rand.Next(0, 1 + 1) == 0 ? 1 : -1) * (float)rand.NextDouble() * maxVelocity.x, 
                (rand.Next(0, 1 + 1) == 0 ? 1 : -1) * (float)rand.NextDouble() * maxVelocity.y);
            part.color = color;
            part.doGravity = false;
            //ageRange.x is lower limit, then multiply random double with upperLimit - lowerLimit (the random range)
            part.maxAge = ageRange.x + (float) rand.NextDouble() * (ageRange.y - ageRange.x); 
        }

        return particles;
    }
    
    public static List<Particle> ClientSpawnEnderShatter(Vector2 position, Color color)
    {
        List<Particle> particles = new List<Particle>();
        Random rand = new Random();
        int particleAmount = 20;
        float circleRadius = 0.1f;
        
        //Calculate angle in radians per particle to evenly space in circle
        float anglePerParticle = 2 * Mathf.PI / particleAmount;
        for (int i = 0; i < particleAmount; i++)
        {
            //Calculate particle x & y in circle
            Vector2 circlePos = new Vector2(
                Mathf.Cos(anglePerParticle * i),
                Mathf.Sin(anglePerParticle * i));
            
            Particle part = ClientSpawn();
            particles.Add(part);
            part.transform.position = position + (circlePos * circleRadius);
            part.velocity = -circlePos * 1.7f;
            part.color = color;
            part.doGravity = false;
            part.maxAge = (float) rand.NextDouble() * 0.6f;
        }
        
        return particles;
    }
    
    public static void ClientSpawnNumber(Vector2 position, int number, Color color)
    {
        Random rand = new Random();
        List<Vector2> shape = new List<Vector2>();

        if (number == 1)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 2)
                , new Vector2(0, 3), new Vector2(-1, 3)
            };
        if (number == 2)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 1)
                , new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3)
            };
        if (number == 3)
            shape = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
                , new Vector2(1, 2), new Vector2(0, 3), new Vector2(-1, 3), new Vector2(1, 3)
            };
        if (number == 4)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, 0), new Vector2(1, 2)
                , new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3)
            };
        if (number == 5)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1)
                , new Vector2(-1, 2), new Vector2(1, 3), new Vector2(-1, 3), new Vector2(0, 3)
            };
        if (number == 6)
            shape = new List<Vector2>
            {
                new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(-1, 1)
                , new Vector2(0, 1), new Vector2(-1, 2), new Vector2(-1, 3)
            };
        if (number == 7)
            shape = new List<Vector2>
            {
                new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3)
                , new Vector2(0, 3)
            };
        if (number > 7)
            shape = new List<Vector2>
            {
                new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 2), new Vector2(1, 3), new Vector2(-1, 3)
                , new Vector2(0, 3), new Vector2(3, 1), new Vector2(4, 3), new Vector2(4, 4), new Vector2(4, 2)
                , new Vector2(5, 3)
            };

        Vector2 totalVelocity = new Vector2(((float) rand.NextDouble() - 0.5f) * 0.5f, (float) rand.NextDouble() * 1f);
        foreach (Vector2 pos in shape)
        {
            Particle part = ClientSpawn();

            part.transform.position = position - new Vector2(0.5f, 0.5f) + pos * 0.25f;
            part.color = color;
            part.doGravity = true;
            part.velocity = totalVelocity;
            part.maxAge = 1f + (float) rand.NextDouble();
            part.maxBounces = 10;
        }
    }
}