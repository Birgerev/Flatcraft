using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Explosion : MonoBehaviour
{
    public float blockDropChance = 0.3f;
    public float radius = 5;
    
    // Start is called before the first frame update
    void Start()
    {
        System.Random r = new System.Random();
        Location loc = Location.LocationByPosition(transform.position, Player.localInstance.Location.dimension);
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, radius);
        List<Block> blocks = new List<Block>();
        List<Entity> entities = new List<Entity>();

        foreach (Collider2D col in objects)
        {
            Block block = col.GetComponent<Block>();
            Entity entity = col.GetComponent<Entity>();
            
            if(block != null)
                blocks.Add(block);
            if(entity != null)
                entities.Add(entity);
        }

        foreach (Block block in blocks)
        {
            if(block.GetMaterial() == Material.TNT)
            {
                ((TNT)block).Prime(0.2f);
                continue;
            }
            
            bool drop = (r.NextDouble() <= blockDropChance);
            
            block.Break(drop);
        }

        foreach (Entity entity in entities)
        {
            Location entityLoc = entity.Location;
            float distance = Vector2.Distance(entityLoc.GetPosition(), loc.GetPosition());
            float damage = (radius - distance) * 7;
            
            entity.Damage(damage);
        }
        
        for (var i = 0; i < 10; i++) //Spawn explosion emmiters
        {
            Particle part = Particle.Spawn();
            var position = loc.GetPosition() + new Vector2(
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -4 : 4),
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -4 : 4));
            float animationTime = 0.2f + ((float) r.NextDouble() * 0.4f);
            float colorBrightness = 0.7f + (float)r.NextDouble() * 0.3f;

            part.transform.position = position;
            part.color = new Color(colorBrightness, colorBrightness, colorBrightness);
            part.doGravity = false;
            part.maxAge = 10;
            
            part.spriteSheet = "particle_explosion";
            part.animationLength = 4;
            part.animationDuration = animationTime;
        }
        
        for (var i = 0; i < 50; i++) //Spawn smoke particles
        {
            Particle part = Particle.Spawn();
            var position = loc.GetPosition() + new Vector2(
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -3 : 3),
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -3 : 3));
            var directionFromCenter = ((Vector2) position - loc.GetPosition()).normalized;
            float animationTime = 1f + ((float) r.NextDouble());
            float colorBrightness = 0.8f + (float)r.NextDouble() * 0.2f;

            part.transform.position = position;
            part.color = new Color(colorBrightness, colorBrightness, colorBrightness);
            part.doGravity = false;
            part.velocity = directionFromCenter * (0 + (float)r.NextDouble() * 2);
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 1;
            
            part.spriteSheet = "particle_smoke";
            part.animationLength = 4;
            part.animationDuration = animationTime;
        }
        
        for (var i = 0; i < 50; i++) //Spawn black particles
        {
            Particle part = Particle.Spawn();
            var position = loc.GetPosition() + new Vector2(
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -4 : 4),
                ((float) r.NextDouble()) * (r.Next(0, 1 + 1) == 0 ? -4 : 4));
            var directionFromCenter = ((Vector2) position - loc.GetPosition()).normalized;
            float colorBrightness = 0.0f + (float)r.NextDouble() * 0.3f;

            part.transform.position = position;
            part.color = new Color(colorBrightness, colorBrightness, colorBrightness);
            part.doGravity = false;
            part.velocity = directionFromCenter * (1 + (float)r.NextDouble() * 2);
            part.maxAge = 1f + (float) r.NextDouble();
            part.maxBounces = 1;
        }

        Sound.Play(loc, "random/explosion", SoundType.Entities, 0.8f, 1.2f, 32);
        
        Destroy(gameObject, 3);
    }

    public static Explosion Create(Location loc, float radius, float blockDropChance)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Explosion");
        GameObject explosionObj = Instantiate(prefab, loc.GetPosition(), Quaternion.identity);
        Explosion explosion = explosionObj.GetComponent<Explosion>();

        explosion.blockDropChance = blockDropChance;
        explosion.radius = radius;

        return explosion;
    }
}
